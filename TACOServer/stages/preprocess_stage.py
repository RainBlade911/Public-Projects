import io
import time
import numpy as np
from PIL import Image

from core.stage import Stage
from core.pipeline_context import PipelineContext

INPUT_H = 513
INPUT_W = 257


class PreprocessStage(Stage):
    """
    Pipeline Stage — converts raw JPEG bytes into a uint8 tensor
    and populates meta (scale, padding, original dimensions).
    Logic is identical to the original preprocess() function.
    """

    def process(self, context: PipelineContext) -> PipelineContext:
        t0 = time.perf_counter()

        
        img = Image.open(io.BytesIO(context.jpeg_bytes)).convert("RGB")
        orig_w, orig_h = img.size

        #Compute scaling
        scale = min(INPUT_W / orig_w, INPUT_H / orig_h)
        new_w = int(orig_w * scale)
        new_h = int(orig_h * scale)

        
        #Resize the image + padding
        img_resized = img.resize((new_w, new_h))
        canvas = Image.new("RGB", (INPUT_W, INPUT_H))
        pad_x = (INPUT_W - new_w) // 2
        pad_y = (INPUT_H - new_h) // 2
        canvas.paste(img_resized, (pad_x, pad_y))

        #convert to tensor
        arr = np.array(canvas).astype(np.uint8)
        arr = np.transpose(arr, (2, 0, 1))
        arr = np.expand_dims(arr, 0)

        context.tensor = arr
        context.meta = {
            "scale":  scale,
            "pad_x":  pad_x,
            "pad_y":  pad_y,
            "orig_w": orig_w,
            "orig_h": orig_h,
        }
        context.timings["preprocess"] = (time.perf_counter() - t0) * 1000
        return context
