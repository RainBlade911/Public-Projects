import collections
from abc import ABC, abstractmethod
from observers.inference_observer import InferenceObserver

# ─────────────────────────────────────────────
# Concrete observer — performance monitor
# ─────────────────────────────────────────────

class PerformanceMonitor(InferenceObserver):
    """
    Observer Pattern (concrete) — subscribes to inference-complete events
    and maintains rolling statistics per stage.
    No longer called imperatively from the handler or pipeline.
    """

    STAGES = ["preprocess", "inference", "decode", "total"]

    def __init__(self, label: str, log_every: int = 30, window: int = 30):
        self.label       = label
        self.log_every   = log_every
        self.frame_count = 0
        self.times = {s: collections.deque(maxlen=window) for s in self.STAGES}

    # ── Observer interface ──────────────────────────────────────────

    def on_inference_complete(self, timings: dict, backend: str):
        for stage, ms in timings.items():
            if stage in self.times:
                self.times[stage].append(ms)
        self.frame_count += 1
        if self.frame_count % self.log_every == 0:
            self._print_summary()

    # ── Internal ────────────────────────────────────────────────────

    def _print_summary(self):
        print(f"\n── [{self.label}] Frame {self.frame_count} ──────────────────────")
        for stage in self.STAGES:
            vals = self.times[stage]
            if not vals:
                continue
            avg = sum(vals) / len(vals)
            fps = 1000 / avg if avg > 0 else 0
        print()