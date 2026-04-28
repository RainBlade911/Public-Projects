from core.pipeline_context import PipelineContext
from stages.preprocess_stage import PreprocessStage
from stages.inference_stage import InferenceStage
from inference.inference_strategy import CPUInferenceRunner


class TestInferenceStage:

    def test_raw_outputs_are_populated(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        ctx = InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        assert ctx.raw_outputs is not None

    def test_raw_outputs_has_two_tensors(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        ctx = InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        assert len(ctx.raw_outputs) == 2

    def test_session_was_called(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        mock_session.run.assert_called_once()

    def test_session_called_with_image_key(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        _, input_dict = mock_session.run.call_args[0]
        assert "image" in input_dict

    def test_inference_timing_is_recorded(self, sample_jpeg, mock_session):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        ctx = PreprocessStage().process(ctx)
        ctx = InferenceStage(mock_session, CPUInferenceRunner()).process(ctx)
        assert "inference" in ctx.timings
        assert ctx.timings["inference"] >= 0