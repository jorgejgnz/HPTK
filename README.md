# HPTK ![](https://img.shields.io/badge/unity-2019.4%20or%20later-green.svg) [![](https://img.shields.io/github/license/jorgejgnz/HPTK.svg)](https://github.com/jorgejgnz/HPTK/LICENSE.md) [![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/jorgejgnz/HPTK?include_prereleases)](https://github.com/jorgejgnz/HPTK/releases) [![](https://img.shields.io/twitter/follow/jorgejgnz.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=jorgejgnz)

Hand Physics Toolkit (HPTK) is a toolkit to build physical hand-driven interactions in a modular and scalable way. Hand physics and hover/touch/grab detection are modules included. This toolkit can be combined with [MRTK-Quest](https://github.com/provencher/MRTK-Quest) for UI interactions. Only Oculus Quest is supported at the moment.

## Main features
- Data model to access hand components and lerp values to compose gestures and trigger actions.
- State-of-the-art hand physics that can be configured in detail through configuration assets.
- Hover/Touch/Grab detection with support to interactions involving multiple objects and hands.
- Code architecture based on isolated modules. Support to custom modules. ([Wiki](https://github.com/jorgejgnz/HPTK/wiki/Custom-modules)).
- Input abstraction. RelativeSkeletonTracker included to mimic other hands. ([Wiki](https://github.com/jorgejgnz/HPTK/wiki/Modules-overview)).

## Example project
- You can clone a ready-to-go project at [HPTK-Sample](https://github.com/jorgejgnz/HPTK-Sample).

[![Demo video](./Documentation/Media/hptk.gif)](https://twitter.com/jorgejgnz/status/1285514990619942912)

# Supported versions
- Unity 2019.4.4f1 LTS, 2019.3.15f1
- [Oculus Integration 20.0](https://developer.oculus.com/downloads/package/unity-integration/)

# Supported target devices
- Oculus Quest - Android

# Getting started with HPTK (Oculus Quest)

1. Obtain *HPTK*.
1. Import *Oculus Integration*.
1. Configure *Build Settings* (Oculus Quest).
1. Configure *Project Settings* (!).
1. Setup a scene with *hand tracking support* (Oculus Quest).
1. Setup *HPTK specific components*.
1. Modify/Create *HPTK Configuration Assets* (if needed).

Checkout the [Wiki](https://github.com/jorgejgnz/HPTK/wiki/Getting-started) for a detailed **step-by-step guide**.

# Wiki
The [Wiki](https://github.com/jorgejgnz/HPTK/wiki) also includes more details about:
- Modules overview.
- Getting started with HPTK.
- How to build new HPTK modules.

# Author
**Jorge Juan González** - *HCI Researcher at I3A (University of Castilla-La Mancha)*

[LinkedIn](https://www.linkedin.com/in/jorgejgnz/) - [Twitter](https://twitter.com/jorgejgnz) - [GitHub](https://github.com/jorgejgnz)

## Acknowledgements

**Oxters Wyzgowski** - [GitHub](https://github.com/oxters168) - [Twitter](https://twitter.com/OxGamesCo)

**Michael Stevenson** - [GitHub](https://github.com/mstevenson)

Nasim, K, Kim, YJ. Physics-based assistive grasping for robust object manipulation in virtual reality. Comput Anim Virtual Worlds. 2018; 29:e1820. [https://doi.org/10.1002/cav.1820](https://doi.org/10.1002/cav.1820)

Linn, Allison. Talking with your hands: How Microsoft researchers are moving beyond keyboard and mouse. The AI Blog. Microsoft. 2016
[https://blogs.microsoft.com/](https://blogs.microsoft.com/ai/talking-hands-microsoft-researchers-moving-beyond-keyboard-mouse/)

# License
[MIT](./LICENSE.md)
