# Quadcopter agents training

### Drone_Raycaster_2.onnx
- **Env:** Road (5M steps)
- **Params:**
  - xx
- **Network:**
  - xx

### Drone_Raycaster_2_pillars_1.onnx
- **Env:** Pillars (5M steps)
- **Params:**
    - batch_size: 128
    - buffer_size: 2048
    - learning_rate: 2.5e-4
    - beta: 8e-3
    - epsilon: 0.2
    - lambd: 0.96
    - num_epoch: 3
    - time_horizon: 86
    - gamma: 0.99
- **Network:**
    - num_layers: 3
    - hidden_units: 86
- **Results:**
  - **Episode length:** линейно растёт от 0 до 200 секунд за 1.5M, стабилизируется на 200-300 секунд после 2.5M.
  - **Cum. reward:** отрицательная до 500k, около нуля до 1M, линейный рост до 180 к 2M, стабилизация около 200 после 2.8M. 
  - **Loss:**
    - Policy: хаотично 
    - Value: рост от 50 до 120 к 1.5M, снижение до 0-30 к 2.5M. 
  - **Collisions:** максимум 160 к 500k, линейное снижение до 0-15 к 2M. 
  - **Waypoints:** линейный рост прогресса от 0 до 1 за 2M, затем стабильно достигает всех точек.

### Drone_Raycaster_2_pillars_1_road.onnx
- **Based on:** *Drone_Raycaster_2_pillars_1.onnx*
- **Env:** Pillars (5M steps) + Road (5 steps)
- **Params:**
  - batch_size: 128
  - buffer_size: 2048
  - learning_rate: 2.5e-4
  - beta: 8e-3
  - epsilon: 0.2
  - lambd: 0.96
  - num_epoch: 3
  - time_horizon: 86
  - gamma: 0.99
- **Network:**
  - num_layers: 3
  - hidden_units: 86
- **Results:**
  - **Episode length:** линейно растёт от 0 до 200 секунд за 1.5M, стабилизируется на 200-300 секунд после 2.5M.
  - **Cum. reward:** отрицательная до 500k, около нуля до 1M, линейный рост до 180 к 2M, стабилизация около 200 после 2.8M. 
  - **Loss:**
  - **Collisions:** максимум 160 к 500k, линейное снижение до 0-15 к 2M.
  - **Waypoints:** линейный рост прогресса от 0 до 1 за 2M, затем стабильно достигает всех точек.

### Drone_Raycaster_2_long_long
- **Env:** Pillars (10M steps), усложнение сцены на 5M - больше препятствий, меньше диапазон допустимых высот
- **Params:**
  - batch_size: 100
  - buffer_size: 10000
  - learning_rate: 0.00025
  - beta: 0.008
  - epsilon: 0.2
  - lambd: 0.96
  - num_epoch: 3
  - time_horizon: 86
  - gamma: 0.99
- **Network:**
  - num_layers: 3
  - hidden_units: 86
- **Results:**
  - **Episode length:** линейно растёт от 0 до 200 секунд за 1.5M, стабилизируется на 200-300 секунд после 2.5M.
  - **Cum. reward:** отрицательная до 500k, около нуля до 1M, линейный рост до 180 к 2M, стабилизация около 200 после 2.8M. Нет ограничений на мин. вознаграждение. После усложнения сцены и снижения некоторых наград (высота) снизилась общая награда, но по-прежнему стабильно более 100.
  - **Loss:**
    - Policy: хаотично
    - Value: рост от 50 до 120 к 1.5M, снижение до 0-30 к 2.5M.
  - **Collisions:** максимум 160 к 500k, линейное снижение до 0-15 к 2M. После усложнения сцены нет значительного роста столкновений.
    - **Waypoints:** линейный рост прогресса от 0 до 1 за 2M, затем стабильно достигает всех точек. После усложнения сцены (5M) больше дисперсия, но стабильно достигает 1.5 - 2.

## Drone_Raycaster_2_pillars_large_batch
- **Env:** Pillars (10M steps)
- **Params:**
  - batch_size: 1000
  - buffer_size: 10000
  - learning_rate: 0.00025
  - beta: 0.008
  - epsilon: 0.2
  - lambd: 0.96
  - num_epoch: 3
  - time_horizon: 86
  - gamma: 0.99
- **Network:**
  - num_layers: 3
  - hidden_units: 86
- **Results:**
  - **Episode length:** медленно растёт от 0 до 200 секунд за 4M, стабилизируется на 150-250 секунд после 5M. Высокая дисперсия
  - **Cum. reward:** очень медленный равномерный рост, положительная после 4M, максимум 70-100 к 9M
  - **Loss:**
    - Policy: хаотично
    - Value: рост от 50 до 120 к 1.5M, снижение до 0-30 к 2.5M.
  - **Collisions:** максимум 160 к 2M, медленное снижение до 5-40 к 6M. После усложнения сцены нет значительного роста столкновений.
    - **Waypoints:** линейный рост прогресса от 0 до 0.75 за 7M
