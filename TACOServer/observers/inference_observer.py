from abc import ABC, abstractmethod
# ─────────────────────────────────────────────
# Observer Pattern — interface
# ─────────────────────────────────────────────

class InferenceObserver(ABC):
    """
    Observer Pattern — any object that wants to react to a completed
    inference event implements this interface and subscribes to the Facade.
    """

    @abstractmethod
    def on_inference_complete(self, timings: dict, backend: str): ...

