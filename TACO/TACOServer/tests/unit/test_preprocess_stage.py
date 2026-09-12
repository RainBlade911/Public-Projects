import numpy as np
from core.pipeline_context import PipelineContext
from stages.preprocess_stage import PreprocessStage, INPUT_H, INPUT_W


class TestPreprocessStage:

    def test_output_tensor_shape(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.tensor.shape == (1, 3, INPUT_H, INPUT_W)

    def test_output_tensor_dtype(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.tensor.dtype == np.uint8

    def test_meta_keys_present(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert "scale" in result.meta
        assert "pad_x" in result.meta
        assert "pad_y" in result.meta
        assert "orig_w" in result.meta
        assert "orig_h" in result.meta

    def test_scale_is_positive(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.meta["scale"] > 0

    def test_padding_is_non_negative(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.meta["pad_x"] >= 0
        assert result.meta["pad_y"] >= 0

    def test_small_image_still_produces_correct_shape(self, small_jpeg):
        ctx = PipelineContext(jpeg_bytes=small_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.tensor.shape == (1, 3, INPUT_H, INPUT_W)

    def test_tall_image_has_horizontal_padding(self, tall_jpeg):
        ctx = PipelineContext(jpeg_bytes=tall_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.meta["pad_x"] > 0

    def test_wide_image_has_vertical_padding(self, wide_jpeg):
        ctx = PipelineContext(jpeg_bytes=wide_jpeg)
        result = PreprocessStage().process(ctx)
        assert result.meta["pad_y"] > 0

    def test_timing_is_recorded(self, sample_jpeg):
        ctx = PipelineContext(jpeg_bytes=sample_jpeg)
        result = PreprocessStage().process(ctx)
        assert "preprocess" in result.timings
        assert result.timings["preprocess"] > 0