# SketchToScroll / AI Draw

**Meta Quest VR app**: sketch in-headset → ComfyUI turns it into AI-generated traditional Chinese ink wash painting (水墨画) → results appear back in VR. Goal: low-cost access to that experience without paper, ink, or formal training.

---

## What it does

- **VR sketchboard**: Draw on a virtual board in Quest with a brush-style controller; strokes are captured as a texture.
- **AI pipeline**: The sketch is sent over HTTP to a local ComfyUI instance, which runs an img2img workflow (e.g. LoRA + KSampler) to produce ink-style artwork.
- **In-VR display**: Generated images are read (e.g. from ComfyUI output folder) and shown on a surface in the same VR scene.

End-to-end: **sketch in VR → AI generation → view result in VR**.

---

## Tech stack

| Layer | Choice |
|-------|--------|
| VR / Client | Unity 2022.3 LTS, Meta XR / Quest SDK, URP |
| Drawing | Runtime `Texture2D` on a plane, raycast-based brush, interpolated strokes |
| AI backend | ComfyUI (local): `/upload/image` + `/prompt` over HTTP; workflow JSON with placeholder for uploaded image name |
| Result display | Folder polling for latest output image → load as texture onto a mesh in scene |

---

## Demo / Showcase

*(Add a short video or screenshots here: Quest user drawing → AI result in VR.)*
