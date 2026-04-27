import time
from abc import ABC, abstractmethod

import onnxruntime as rt

from core.pipeline_context import PipelineContext


# ─────────────────────────────────────────────
# Strategy Pattern — common interface
# ─────────────────────────────────────────────

class InferenceStrategy(ABC):
    """
    Strategy Pattern — defines the interface every backend must satisfy.
    The InferenceStage holds a reference to one of these and calls run(),
    without ever knowing whether it's talking to the NPU or CPU.
    """

    @abstractmethod
    def run(self, tensor, sess) -> list:
        """Execute the ONNX session and return raw outputs."""
        ...


# ─────────────────────────────────────────────
# Template Method Pattern — base runner
# ─────────────────────────────────────────────

class InferenceRunner(InferenceStrategy):
    """
    Template Method Pattern — defines the invariant skeleton:
        1. start timer
        2. call _run_session()   ← hook overridden by subclasses
        3. stop timer
    Subclasses only override _run_session(); timing logic lives here once.
    """

    def run(self, tensor, sess) -> tuple[list, float]:
        t0 = time.perf_counter()
        outputs = self._run_session(tensor, sess)
        inf_ms = (time.perf_counter() - t0) * 1000
        return outputs, inf_ms

    @abstractmethod
    def _run_session(self, tensor, sess) -> list:
        ...


# ─────────────────────────────────────────────
# Concrete strategies
# ─────────────────────────────────────────────

class NPUInferenceRunner(InferenceRunner):
    """Strategy: Qualcomm NPU via QNNExecutionProvider."""

    def _run_session(self, tensor, sess) -> list:
        return sess.run(None, {"image": tensor})


class CPUInferenceRunner(InferenceRunner):
    """Strategy: plain CPU via CPUExecutionProvider."""

    def _run_session(self, tensor, sess) -> list:
        return sess.run(None, {"image": tensor})