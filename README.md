# DiminishedReality
A Unity3D Diminished Reality App based on ARFoundation using an external CNN via Python Server for Inpainting. 

# Project Setup

Unity:
- download or clone this unity project
- open up the project in unity (2022.2.2f1) and switch platform to ios
- build the app to your device (deactivate bitcode in XCode)

Python:
- download or clone the [LaMa: Resolution-robust Large Mask Inpainting with Fourier Convolutions](https://github.com/advimman/lama) repository
- install a python environment with e.g. pyenv 
- install C++ Build Tools (Build for Desktop)
- On Windows: install Nvidia CUDA Toolkit 
- activate your environment and install the requierement.txt
- paste the subfolders inside "lama" from this repo to your lama folder
- create two folders inside the "lama" folder named "fromdevice" and "output"
- launch the server.py in your environment

App:
- launche the app on your device 
- consider that the server machine and the device are in the same local network
- enter the ip adress from the server host into the app
- launch a scene trough the main menu (LiDAR Scene is only available on a compatible device)

# Acknowledgement
[LaMa: Resolution-robust Large Mask Inpainting with Fourier Convolutions](https://github.com/advimman/lama)
 
