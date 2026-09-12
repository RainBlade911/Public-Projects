from dataclasses import dataclass, field
from typing import Any, Optional
import numpy as np


@dataclass
class PipelineContext:
    """Shared data object passed through every pipeline stage."""

    # --- Input ---
    jpeg_bytes: bytes = b""

    # --- Preprocess outputs ---
    tensor: Optional[np.ndarray] = None
    meta: Optional[dict] = None

    # --- Inference outputs ---
    raw_outputs: Optional[list] = None
    inf_ms: float = 0.0

    # --- Decode outputs ---
    keypoints: Optional[list] = None

    # --- Timing (filled by each stage) ---
    timings: dict = field(default_factory=dict)