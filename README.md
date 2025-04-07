# Unity Drone Simulator

My attempt on creating self-flying quadcopter in Unity with ML-Agents.


## Features
This project is not finished yet, so many things can and will be added and improved. For now there are several features that I have implemented.

**PID stabilized controls**

Qadcopter takes 4 float values as input. They are *throttle*, *pitch*, *yaw* and *roll*. To make drone more stable these controls are being fed to corresponding [PID controllers](https://en.wikipedia.org/wiki/Proportional%E2%80%93integral%E2%80%93derivative_controller) before the actual motors. For each control value there is a dedicated PID:
1. _Throttle_ PID stabilizes climb speed so drone will stay at one height if no controls are applied and will ascend/descend with constant speed if control is positive or negative.
2. _Pitch_ and _Roll_ PIDs stabilze rotation along X and Z axis (see Unity's coordinates system). This will prevent drone from overturning.
3. _Yaw_ PID stabilizes rotation speed along Y axis.

**Quadcopter components destruction**

Four drone propellers can be destroyed and detached from the frame if collided with enough force. Drone can barely fly and control itself with broken motors.

**Depth map sensor**

Scene depth can be observed using URP full screen shader. This shader is than used in ML Agents' `RenderTextureSensor` component to send depth date to the neural network.


## Examples
Playable drone UI:

<img width="789" alt="Playable drone UI" src="https://github.com/user-attachments/assets/2fe9b63a-ecf9-4bfd-a30f-814ccda44f44" />

Raycast AI drone prefab:

<img width="789" alt="Lidar drone" src="https://github.com/user-attachments/assets/49cea489-74f0-42cd-982c-bac963b7563e" />

GridSensor AI drone prefab (thanks to [mbaske](https://github.com/mbaske/grid-sensor) repository):

<img width="789" alt="Снимок экрана 2025-04-08 в 01 05 20" src="https://github.com/user-attachments/assets/89ea7227-7f7d-458f-8cba-0762bfc0c545" />
<img width="453" alt="Снимок экрана 2025-04-08 в 01 05 34" src="https://github.com/user-attachments/assets/391e6d24-0ebc-444f-8bfb-00dee0a61152" />

Sample scene view:

<img width="789" alt="Снимок экрана 2025-04-08 в 01 01 55" src="https://github.com/user-attachments/assets/e3b0ade9-5904-4033-89b3-041da86d3de2" />

