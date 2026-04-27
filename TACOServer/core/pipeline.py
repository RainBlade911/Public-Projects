from typing import List
from core.stage import Stage
from core.pipeline_context import PipelineContext


class Pipeline:
    """
    Pipeline Pattern — owns an ordered list of stages and runs them in sequence.
    Output of each stage feeds directly into the next via the shared context.
    The Facade uses this to orchestrate preprocess → inference → decode.
    """

    def __init__(self, stages: List[Stage]):
        self._stages = stages

    def run(self, context: PipelineContext) -> PipelineContext:
        for stage in self._stages:
            context = stage.process(context)
        return context