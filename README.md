# TACO — Real-Time Pose Estimation on Snapdragon NPU

Real-time human pose estimation running on the Qualcomm Hexagon NPU via ONNX Runtime QNN Execution Provider, with a Unity frontend for visualisation and avatar driving.

## Architecture

```
TACOUnity/      Unity 6 project — webcam feed, pose overlay, avatar
TACOPlugin/     C++ native plugin — preprocessing, NPU inference, decoding
TACOServer/     Python WebSocket server — reference backend for validation
```

The native C++ plugin is the primary inference path (~4ms on NPU). The Python WebSocket server is a CPU/NPU reference implementation used for development and comparison (~200ms round trip).

---

## Prerequisites

| Component | Requirement |
|-----------|-------------|
| Hardware  | Snapdragon X Elite (X1E80100) — NPU path only. CPU fallback works on any Windows ARM64 machine. |
| Unity     | Unity 6 (6000.3.x LTS) |
| Visual Studio | VS 2022 with C++ Desktop and ARM64 build tools |
| Python    | 3.11 ARM64 — [download ARM64 installer from python.org](https://www.python.org/downloads/windows/) |

All required DLLs (QNN, ONNX Runtime, OpenCV) and the ONNX model are included in the repo — no additional SDK downloads needed.

---

## Setup

### 1. Clone the repo

```powershell
git clone <repo-url>
cd TACO
```

---

### 2. Unity

Open `TACOUnity/` in Unity 6.

All required DLLs are already in `TACOUnity/Assets/Plugins/` — Unity will pick them up automatically.

In the scene, select the **PoseManager** GameObject and configure:
- `PoseBackendController` → check **Use Plugin** for native NPU/CPU, uncheck for WebSocket
- `NativePluginBackend` → toggle **Use NPU** (requires Snapdragon X Elite)
- `WebSocketBackend` → **Server Url** defaults to `ws://localhost:8765`

Hit Play.

---

### 3. Python WebSocket Server (optional — for reference/comparison)

```powershell
cd TACOServer
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
```

**Configure the QNN DLL path** by setting an environment variable:

```powershell
$env:QNN_HTP_DLL = "C:\path\to\QnnHtp.dll"
```

The DLL from the repo can be used directly:
```powershell
$env:QNN_HTP_DLL = "..\TACOPlugin\Dependencies\QnnHtp.dll"
```

To switch to CPU (no NPU required):
```python
# In app.py
USE_NPU = False
```

Run the server:
```powershell
python app.py
```

Then in Unity set `PoseBackendController` → **Use Plugin** to unchecked.

---

### 4. Building the C++ Plugin (only needed if modifying plugin code)

Team members who only work on Unity or Python do not need to do this — the compiled `TACOPlugin.dll` is already in `TACOUnity/Assets/Plugins/`.

If you modify any C++ source:

1. Open `TACOPlugin/TACOPlugin.sln` in Visual Studio 2022
2. Set configuration to **Release / ARM64**
3. Build the solution
4. Copy `TACOPlugin/ARM64/Release/TACOPlugin.dll` to `TACOUnity/Assets/Plugins/`

---

## Backend Comparison

| Backend | Inference | Total | Notes |
|---------|-----------|-------|-------|
| NPU Plugin | ~1–4ms | ~4ms | Snapdragon X Elite required |
| CPU Plugin | ~80ms | ~80ms | Any ARM64 Windows |
| WebSocket NPU | ~4ms | ~200ms | Round trip overhead |
| WebSocket CPU | ~80ms | ~200ms | Round trip overhead |

---

## Project Structure

```
TACO/
├── TACOUnity/                  Unity project
│   └── Assets/
│       ├── Plugins/            TACOPlugin.dll + all runtime DLLs
│       ├── StreamingAssets/    model.onnx
│       └── Scripts/            C# source
├── TACOPlugin/                 C++ Visual Studio project
│   ├── Dependencies/           Headers and libs for building
│   └── *.cpp / *.h             Plugin source
├── TACOServer/                 Python WebSocket server
│   ├── app.py
│   ├── model.onnx
│   └── requirements.txt
├── .gitignore
└── README.md
```

---

## Known Issues

- **NPU driver state**: If the NPU stops responding after a crash or sleep, reboot the machine. If the issue persists, reinstall the Qualcomm NPU driver from [Dell support](https://www.dell.com/support/home/en-us/product-support/product/xps-13-9345/drivers).
- **Unity Editor crash**: `TACOPlugin.dll` must be set to **ARM64 standalone only** in the Plugin Inspector — do not enable it for Editor. Loading it in the Editor will crash Unity.
- **WebSocket perf overlay**: Pre/Inf breakdown requires the Python server to be running. If values show 0, check the server is running and connected.
- **CPU fallback**: The quantized uint8 model produces correct results on CPU but runs ~20x slower than NPU.
