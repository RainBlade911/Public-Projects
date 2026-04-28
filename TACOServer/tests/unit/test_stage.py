import pytest
from core.stage import Stage


def test_stage_is_abstract():
    with pytest.raises(TypeError):
        Stage()

def test_subclass_without_process_fails():
    class BadStage(Stage):
        pass

    with pytest.raises(TypeError):
        BadStage()

from core.pipeline_context import PipelineContext


def test_valid_stage_runs_process():
    class GoodStage(Stage):
        def process(self, context: PipelineContext) -> PipelineContext:
            context.timings["ok"] = True
            return context

    ctx = PipelineContext()
    stage = GoodStage()

    result = stage.process(ctx)

    assert result.timings["ok"] is True

def test_stage_returns_context_instance():
    class GoodStage(Stage):
        def process(self, context: PipelineContext) -> PipelineContext:
            return context

    ctx = PipelineContext()
    stage = GoodStage()

    result = stage.process(ctx)

    assert isinstance(result, PipelineContext)