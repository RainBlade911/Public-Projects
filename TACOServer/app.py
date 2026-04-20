import asyncio
import json
import io
import time
import collections


import numpy as np
import onnxruntime as rt
from PIL import Image
import websockets


import onnxruntime as rt




MODEL_PATH = "model.onnx"

INPUT_H = 513
INPUT_W = 257

HEAT_SCALE = 0.00390625
HEAT_ZERO  = 178

OFF_SCALE  = 0.08
OFF_ZERO   = 127

STRIDE_Y = INPUT_H / 33
STRIDE_X = INPUT_W / 17

USE_NPU = True # ← flip this to switch backend


# ---------------- PERFORMANCE MONITOR ----------------

class PerformanceMonitor:
    STAGES = ["preprocess", "inference", "decode", "total"]

    def __init__(self, label, log_every=30, window=30):
        self.label       = label
        self.log_every   = log_every
        self.frame_count = 0
        self.times = {s: collections.deque(maxlen=window) for s in self.STAGES}

    def record(self, stage_times: dict):
        for stage, ms in stage_times.items():
            self.times[stage].append(ms)
        self.frame_count += 1
        if self.frame_count % self.log_every == 0:
            self._print_summary()

    def _print_summary(self):
        print(f"\n── [{self.label}] Frame {self.frame_count} ──────────────────────")
        for stage in self.STAGES:
            vals = self.times[stage]
            if not vals:
                continue
            avg = sum(vals) / len(vals)
            fps = 1000 / avg if avg > 0 else 0
            #print(f"  {stage:<12} avg={avg:6.1f}ms  min={min(vals):5.1f}  max={max(vals):5.1f}  ({fps:.1f} fps)")
        print()


# ---------------- SESSION LOADERS ----------------

def load_npu_session():
    print("Loading NPU session...")
    options = rt.SessionOptions()
    options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_ENABLE_ALL
    provider_options = {
       "backend_path": "QnnHtp.dll",   
        "htp_performance_mode": "burst",
        "profiling_level": "off",
        "htp_arch":             "v73",  
    }
    sess = rt.InferenceSession(
        MODEL_PATH,
        sess_options=options,
        providers=["QNNExecutionProvider", "CPUExecutionProvider"],
        provider_options=[provider_options, {}],
    )
    print("NPU session ready. Providers:", sess.get_providers())
    return sess


def load_cpu_session():
    print("Loading CPU session...")
    options = rt.SessionOptions()
    options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_DISABLE_ALL  
    sess = rt.InferenceSession(
        MODEL_PATH,
        sess_options=options,
        providers=["CPUExecutionProvider"],
    )
    print("CPU session ready. Providers:", sess.get_providers())
    return sess


# ---------------- PREPROCESS ----------------

def preprocess(jpeg_bytes):
    img = Image.open(io.BytesIO(jpeg_bytes)).convert("RGB")

    orig_w, orig_h = img.size
    scale  = min(INPUT_W / orig_w, INPUT_H / orig_h)
    new_w  = int(orig_w * scale)
    new_h  = int(orig_h * scale)

    img_resized = img.resize((new_w, new_h))
    canvas = Image.new("RGB", (INPUT_W, INPUT_H))
    pad_x  = (INPUT_W - new_w) // 2
    pad_y  = (INPUT_H - new_h) // 2
    canvas.paste(img_resized, (pad_x, pad_y))

    arr = np.array(canvas).astype(np.uint8)
    arr = np.transpose(arr, (2, 0, 1))
    arr = np.expand_dims(arr, 0)

    meta = {
        "scale":  scale,
        "pad_x":  pad_x,
        "pad_y":  pad_y,
        "orig_w": orig_w,
        "orig_h": orig_h,
    }
    return arr, meta


# ---------------- POSE DECODER ----------------

def decode_pose(outputs, meta):
    heat_u8 = outputs[0][0]
    off_u8  = outputs[1][0]

    heat = (heat_u8.astype(np.float32) - HEAT_ZERO) * HEAT_SCALE
    offs = (off_u8.astype(np.float32)  - OFF_ZERO)  * OFF_SCALE

    print("heat shape: ",heat.shape)

    keypoints = []
    for k in range(17):
        idx   = np.argmax(heat[k])
        y, x  = np.unravel_index(idx, heat[k].shape)
        score = float(heat[k, y, x])

        px = (x + 0.5) * STRIDE_X + offs[k+17, y, x]
        py = (y + 0.5) * STRIDE_Y + offs[k,    y, x]
        print("offset x:", offs[k+17, y, x])
        print("offset y:", offs[k, y, x])
        print("raw off:", off_u8[k, y, x])

        px = (px - meta["pad_x"]) / meta["scale"]
        py = (py - meta["pad_y"]) / meta["scale"]

        keypoints.append({
            "id":    k,
            "x":     float(px / meta["orig_w"]),
            "y":     float(py / meta["orig_h"]),
            "score": score,
        })

        print(f"id = {id}   x = {x}   y = {y}    score = {score}")

    return keypoints


# ---------------- INFERENCE FUNCTIONS ----------------

def run_on_npu(tensor, meta, sess):
    t0        = time.perf_counter()
    outputs   = sess.run(None, {"image": tensor})
    inf_ms    = (time.perf_counter() - t0) * 1000
    keypoints = decode_pose(outputs, meta)
    return keypoints, inf_ms


def run_on_cpu(tensor, meta, sess):
    t0        = time.perf_counter()
    outputs   = sess.run(None, {"image": tensor})
    inf_ms    = (time.perf_counter() - t0) * 1000
    keypoints = decode_pose(outputs, meta)
    return keypoints, inf_ms


# ---------------- WEBSOCKET HANDLER ----------------

async def handler(ws, sess, monitor, backend_label):
    print(f"Client connected — running on {backend_label}")

   

    async for message in ws:
        

        try:
            t_start = time.perf_counter()

            tensor, meta = preprocess(message)
            t_pre = time.perf_counter()

            if USE_NPU:
                keypoints, inf_ms = run_on_npu(tensor, meta, sess)
            else:
                keypoints, inf_ms = run_on_cpu(tensor, meta, sess)

            t_end = time.perf_counter()

            pre_ms   = (t_pre  - t_start) * 1000
            dec_ms   = (t_end  - t_pre)   * 1000 - inf_ms
            total_ms = (t_end  - t_start) * 1000

            monitor.record({
                "preprocess": pre_ms,
                "inference":  inf_ms,
                "decode":     dec_ms,
                "total":      total_ms,
            })

            payload = {
                "latency_ms": total_ms,
                "keypoints":  keypoints,
                "backend":    backend_label,
                "perf": {
                    "pre_ms":   round(pre_ms,   1),
                    "inf_ms":   round(inf_ms,   1),
                    "total_ms": round(total_ms, 1),
                },
            }

            await ws.send(json.dumps(payload))

        except Exception as e:
            print("Error:", e)


# ---------------- START SERVER ----------------

async def main():
    if USE_NPU:
        sess          = load_npu_session()
        monitor       = PerformanceMonitor("NPU")
        backend_label = "NPU"
    else:
        sess          = load_cpu_session()
        monitor       = PerformanceMonitor("CPU")
        backend_label = "CPU"

    server = await websockets.serve(
        lambda ws: handler(ws, sess, monitor, backend_label),
        "localhost",
        8765,
    )

    print(f"PoseNet server on ws://localhost:8765  |  backend = {backend_label}")
    await server.wait_closed()


try:
    asyncio.run(main())
except Exception as e:
    import traceback
    print("FATAL ERROR:")
    traceback.print_exc()
    input("Press enter to exit...")