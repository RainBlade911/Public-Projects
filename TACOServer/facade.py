import time
from typing import List

from core.pipeline import Pipeline
from core.pipeline_context import PipelineContext
from stages.preprocess_stage import PreprocessStage
from stages.inference_stage import InferenceStage
from stages.decode_stage import DecodeStage
from observers.performance_monitor import InferenceObserver


class PoseEstimationFacade:
    """
    Facade Pattern — single public interface for the entire pose-estimation
    pipeline. The WebSocket handler calls estimate() and receives a ready-to-
    serialise result dict; it never touches stages, strategies, or sessions.

    Observer Pattern — holds a list of InferenceObserver subscribers and
    notifies them after every completed pipeline run.
    """

    def __init__(self, sess, runner, backend_label: str):
        self._backend_label = backend_label
        self._observers: List[InferenceObserver] = []

        # Build the pipeline once; stages are stateless so it's reusable.
        self._pipeline = Pipeline([
            PreprocessStage(),
            InferenceStage(sess, runner),
            DecodeStage(),
        ])

    # ── Observer subscription ────────────────────────────────────────

    def subscribe(self, observer: InferenceObserver):
        self._observers.append(observer)

    def unsubscribe(self, observer: InferenceObserver):
        self._observers.remove(observer)

    # ── Public API ───────────────────────────────────────────────────

    def estimate(self, jpeg_bytes: bytes) -> dict:
        t_start = time.perf_counter()

        ctx = PipelineContext(jpeg_bytes=jpeg_bytes)
        ctx = self._pipeline.run(ctx)

        total_ms = (time.perf_counter() - t_start) * 1000
        ctx.timings["total"] = total_ms

        self._notify_observers(ctx.timings)

        return {
            "latency_ms": total_ms,
            "keypoints":  ctx.keypoints,
            "backend":    self._backend_label,
            "perf": {
                "pre_ms":   round(ctx.timings.get("preprocess", 0), 1),
                "inf_ms":   round(ctx.timings.get("inference",  0), 1),
                "total_ms": round(total_ms, 1),
            },
        }

    # ── Internal ─────────────────────────────────────────────────────

    def _notify_observers(self, timings: dict):
        for observer in self._observers:
            observer.on_inference_complete(timings, self._backend_label)