from core.pipeline import Pipeline
from core.pipeline_context import PipelineContext
from core.stage import Stage


class DoubleStage(Stage):
    """A simple test stage that just records it was called."""
    def __init__(self, name):
        self.name = name

    def process(self, ctx):
        ctx.timings[self.name] = 1.0
        return ctx


class TestPipeline:

    def test_single_stage_runs(self):
        ctx = PipelineContext()
        result = Pipeline([DoubleStage("a")]).run(ctx)
        assert "a" in result.timings

    def test_stages_run_in_order(self):
        order = []

        class TrackStage(Stage):
            def __init__(self, label):
                self.label = label
            def process(self, ctx):
                order.append(self.label)
                return ctx

        Pipeline([TrackStage("a"), TrackStage("b"), TrackStage("c")]).run(PipelineContext())
        assert order == ["a", "b", "c"]

    def test_all_stage_timings_present(self):
        ctx = PipelineContext()
        result = Pipeline([
            DoubleStage("preprocess"),
            DoubleStage("inference"),
            DoubleStage("decode"),
        ]).run(ctx)
        assert "preprocess" in result.timings
        assert "inference" in result.timings
        assert "decode" in result.timings

    def test_empty_pipeline_returns_context(self):
        ctx = PipelineContext(jpeg_bytes=b"hello")
        result = Pipeline([]).run(ctx)
        assert result.jpeg_bytes == b"hello"

    def test_returns_pipeline_context(self):
        result = Pipeline([DoubleStage("a")]).run(PipelineContext())
        assert isinstance(result, PipelineContext)