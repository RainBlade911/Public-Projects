from abc import ABC, abstractmethod
from core.pipeline_context import PipelineContext


class Stage(ABC):
    """
    Pipeline Pattern — base interface for every processing stage.
    Each stage receives a shared PipelineContext, transforms it in-place,
    and returns it so the next stage can be chained.
    """

    @abstractmethod
    def process(self, context: PipelineContext) -> PipelineContext:
        ...