from core.pipeline_context import PipelineContext


def test_default_values():
    ctx = PipelineContext()

    assert ctx.jpeg_bytes == b""
    assert ctx.tensor is None
    assert ctx.meta is None
    assert ctx.raw_outputs is None
    assert ctx.inf_ms == 0.0
    assert ctx.keypoints is None
    assert ctx.timings == {}

def test_custom_jpeg_bytes():
    data = b"fake_image"
    ctx = PipelineContext(jpeg_bytes=data)

    assert ctx.jpeg_bytes == data

def test_mutation_across_stages():
    ctx = PipelineContext()

    ctx.tensor = "dummy_tensor"
    ctx.meta = {"scale": 1.0}
    ctx.raw_outputs = ["out1", "out2"]
    ctx.keypoints = [{"id": 0}]
    ctx.timings["test"] = 123

    assert ctx.tensor == "dummy_tensor"
    assert ctx.meta["scale"] == 1.0
    assert len(ctx.raw_outputs) == 2
    assert ctx.keypoints[0]["id"] == 0
    assert ctx.timings["test"] == 123

def test_timings_are_not_shared_between_instances():
    ctx1 = PipelineContext()
    ctx2 = PipelineContext()

    ctx1.timings["a"] = 1

    assert "a" not in ctx2.timings