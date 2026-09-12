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

Install these before cloning the repo:

| Tool | Download |
|------|----------|
| Unity 6 (6000.3.x LTS) | [unity.com/download](https://unity.com/download) |
| Git + Git LFS | [git-scm.com](https://git-scm.com) — run `git lfs install` after installing |
| Python 3.11 ARM64 | [python.org/downloads/windows](https://www.python.org/downloads/windows/) — make sure to pick the ARM64 installer |
| Visual Studio 2022 | Only needed if modifying C++ plugin code |

> **Hardware note:** The NPU backend requires a Snapdragon X Elite (X1E80100) machine (Dell XPS 13 9345 or similar). The CPU fallback works on any Windows ARM64 machine.

All required DLLs (QNN, ONNX Runtime, OpenCV) and the ONNX model are already included in the repo — no additional SDK downloads needed.

---

## Getting Started

### Step 1 — Clone the repo

Git LFS must be installed before cloning, otherwise large DLL files will be missing.

```powershell
git lfs install
git clone https://github.com/TrietLe347/TACO.git
cd TACO
```

If you already cloned without LFS, run:
```powershell
git lfs pull
```

---

### Step 2 — Open the Unity project

1. Open **Unity Hub**
2. Click **Add** → **Add project from disk**
3. Select the `TACOUnity/` folder
4. Open the project in Unity 6 (6000.3.x LTS)
5. Wait for Unity to finish importing assets (first time takes a few minutes)
6. Open `Assets/Scenes/SampleScene`

All required DLLs are already in `TACOUnity/Assets/Plugins/Windows/ARM64/` — Unity picks them up automatically.

---

### Step 3 — Configure the scene

In the **Hierarchy** panel, find the **PoseManager** GameObject. In the Inspector you will see:

**PoseBackendController**
- `Use Plugin` — check this to use the native C++ plugin (NPU or CPU), uncheck to use the Python WebSocket server

**NativePluginBackend**
- `Model Path` — leave as `model.onnx` (file is in `StreamingAssets/`)
- `Use NPU` — check for NPU (Snapdragon X Elite required), uncheck for CPU fallback

**WebSocketBackend**
- `Server Url` — leave as `ws://localhost:8765` unless running the server on another machine

---

### Step 4 — Run the project

**Option A — Native Plugin (recommended)**
1. In `PoseBackendController` check **Use Plugin**
2. In `NativePluginBackend` set **Use NPU** based on your hardware
3. Make sure a webcam is connected
4. Hit **Play**

**Option B — Python WebSocket Server**
1. Set up the Python server (see Step 5 below) and make sure it is running
2. In `PoseBackendController` uncheck **Use Plugin**
3. Hit **Play**

---

### Step 5 — Python WebSocket Server (optional)

Only needed if using the WebSocket backend or doing server-side development.

**Install dependencies:**
```powershell
cd TACOServer
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
```

**Set the QNN DLL path** (required for NPU mode, skip if using CPU):

The `QnnHtp.dll` is included in the repo. Point to it using an environment variable:
```powershell
$env:QNN_HTP_DLL = "..\TACOPlugin\ARM64\Release\QnnHtp.dll"
```

To use CPU instead of NPU, open `app.py` and change:
```python
USE_NPU = False
```

**Run the server:**
```powershell
python app.py
```

You should see:
```
PoseNet server on ws://localhost:8765  |  backend = NPU
```

Leave this terminal running, then hit Play in Unity.

---

### Step 6 — Building the C++ Plugin (only if modifying plugin code)

> Most team members will not need this — the compiled `TACOPlugin.dll` is already in the repo.

If you change any `.cpp` or `.h` files in `TACOPlugin/`:

1. Install **Visual Studio 2022** with the **Desktop development with C++** workload and **ARM64 build tools**
2. Open `TACOPlugin/TACOPlugin.sln`
3. Set configuration to **Release / ARM64** (top toolbar dropdown)
4. Build → Build Solution (`Ctrl+Shift+B`)
5. Copy the output DLL to Unity:
```powershell
Copy-Item "TACOPlugin\ARM64\Release\TACOPlugin.dll" "TACOUnity\Assets\Plugins\Windows\ARM64\TACOPlugin.dll"
```

---

## Backend Comparison

| Backend | Inference | Total | Notes |
|---------|-----------|-------|-------|
| NPU Plugin | ~1–4ms | ~4ms | Snapdragon X Elite required |
| CPU Plugin | ~80ms | ~80ms | Any ARM64 Windows |
| WebSocket NPU | ~4ms | ~200ms | Round trip + network overhead |
| WebSocket CPU | ~80ms | ~200ms | Round trip + network overhead |

---

## Project Structure

```
TACO/
├── TACOUnity/                  Unity project
│   └── Assets/
│       ├── Plugins/
│       │   └── Windows/ARM64/  TACOPlugin.dll + all runtime DLLs
│       ├── StreamingAssets/    model.onnx
│       └── Scripts/            C# source files
├── TACOPlugin/                 C++ Visual Studio project
│   ├── ARM64/Release/          Compiled DLL output
│   └── *.cpp / *.h             Plugin source
├── TACOServer/                 Python WebSocket server
│   ├── app.py                  Server entry point
│   ├── model.onnx              ONNX model
│   └── requirements.txt        Python dependencies
├── .gitattributes
├── .gitignore
└── README.md
```

---

## Known Issues

- **NPU not working after crash or sleep**: Reboot the machine. If it still fails after reboot, reinstall the Qualcomm NPU driver from [Dell support](https://www.dell.com/support/home/en-us/product-support/product/xps-13-9345/drivers).
- **Unity crashes on open**: `TACOPlugin.dll` must be configured as **ARM64 standalone only** in the Plugin Inspector. If you accidentally enable it for Editor it will crash Unity on startup. Fix: rename `TACOPlugin.dll` to `TACOPlugin.dll.bak` outside of Unity, open the project, configure the plugin settings, rename it back.
- **No keypoints showing**: Check the Console for errors. Most likely cause is the model file missing from `StreamingAssets/` or the wrong backend selected.
- **WebSocket perf overlay shows 0ms**: The Python server must be running and connected. Check the terminal for errors.
- **CPU fallback**: Works on any machine but runs ~20x slower than NPU. Confidence scores may differ slightly from NPU due to floating point vs quantized execution.