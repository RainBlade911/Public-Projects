import asyncio
import json
import websockets

from inference.session_factory import NPUSessionFactory, CPUSessionFactory
from inference.inference_strategy import NPUInferenceRunner, CPUInferenceRunner
from observers.performance_monitor import PerformanceMonitor
from facade import PoseEstimationFacade

USE_NPU = False  # ← flip to switch backend


# ─────────────────────────────────────────────
# WebSocket handler — now paper-thin
# ─────────────────────────────────────────────

async def handler(ws, facade: PoseEstimationFacade):
    print(f"Client connected — backend: {facade._backend_label}")

    async for message in ws:
        try:
            result = facade.estimate(message)
            await ws.send(json.dumps(result))
        except Exception as e:
            print("Error:", e)


# ─────────────────────────────────────────────
# Wiring — Factory + Strategy selection
# ─────────────────────────────────────────────

def build_facade() -> PoseEstimationFacade:
    """
    Factory Method — picks the right SessionFactory and InferenceRunner
    based on USE_NPU, then wires them into the Facade.
    """
    if USE_NPU:
        sess          = NPUSessionFactory().create()
        runner        = NPUInferenceRunner()
        backend_label = "NPU"
    else:
        sess          = CPUSessionFactory().create()
        runner        = CPUInferenceRunner()
        backend_label = "CPU"

    facade  = PoseEstimationFacade(sess, runner, backend_label)
    monitor = PerformanceMonitor(backend_label)
    facade.subscribe(monitor)          # Observer subscription
    return facade


# ─────────────────────────────────────────────
# Entry point
# ─────────────────────────────────────────────

async def main():
    facade = build_facade()

    server = await websockets.serve(
        lambda ws: handler(ws, facade),
        "localhost",
        8765,
    )

    print(f"PoseNet server on ws://localhost:8765  |  backend = {facade._backend_label}")
    await server.wait_closed()


try:
    asyncio.run(main())
except Exception as e:
    import traceback
    print("FATAL ERROR:")
    traceback.print_exc()
    input("Press enter to exit...")