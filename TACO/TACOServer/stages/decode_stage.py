import time
import numpy as np

from core.stage import Stage
from core.pipeline_context import PipelineContext

HEAT_SCALE = 0.00390625
HEAT_ZERO  = 178
OFF_SCALE  = 0.08
OFF_ZERO   = 127

INPUT_H = 513
INPUT_W = 257
STRIDE_Y = INPUT_H / 33
STRIDE_X = INPUT_W / 17


class DecodeStage(Stage):
    """
    Pipeline Stage — dequantises heatmaps and offsets, finds the peak
    per keypoint, and maps pixel coords back to normalised [0, 1] space.
    Logic is identical to the original decode_pose() function.
    """

    def process(self, context: PipelineContext) -> PipelineContext:
        t0 = time.perf_counter()

        heat_u8 = context.raw_outputs[0][0]
        off_u8  = context.raw_outputs[1][0]
        meta    = context.meta

        heat = (heat_u8.astype(np.float32) - HEAT_ZERO) * HEAT_SCALE
        offs = (off_u8.astype(np.float32)  - OFF_ZERO)  * OFF_SCALE

        print("heat shape:", heat.shape)

        keypoints = []
        for k in range(17):
            idx   = np.argmax(heat[k])
            y, x  = np.unravel_index(idx, heat[k].shape)
            score = float(heat[k, y, x])

            px = (x + 0.5) * STRIDE_X + offs[k + 17, y, x]
            py = (y + 0.5) * STRIDE_Y + offs[k,      y, x]

            #print("offset x:", offs[k + 17, y, x])
            #print("offset y:", offs[k, y, x])
            #print("raw off:",  off_u8[k, y, x])

            px = (px - meta["pad_x"]) / meta["scale"]
            py = (py - meta["pad_y"]) / meta["scale"]

            x_norm = px / meta["orig_w"]
            y_norm = py / meta["orig_h"]

            x_norm = max(0.0, min(1.0, x_norm))
            y_norm = max(0.0, min(1.0, y_norm))

            keypoints.append({
                "id":    k,
                "x":     float(x_norm),
                "y":     float(y_norm),
                "score": score,
            })

            #print(f"id={k}  x={x}  y={y}  score={score:.4f}")

        context.keypoints = keypoints
        context.timings["decode"] = (time.perf_counter() - t0) * 1000
        return context