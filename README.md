# LED Matrix Simulator
This project should help to build new "Faces" for a LED Matrix without needing to connect directly to it. It uses a NamedPipeServer/Client to get data from the "real" project.

The sample shows the time, some weather data and a rain chart (if it rains) or a weather forecast.

![LedMatrixSimulator](https://github.com/marcogruhl/LedMatrixSimulator/assets/9695930/0634be75-8ed5-431e-8dec-da9914aebee6)

# Goals

- no need for connecting to actual matrix
- faster develpment
- hot reload
- debugging

# Future development

- proxy to client
- proxy from client

# Why?

I am using it for C# development with the [rpi-rgb-led-matrix](https://github.com/hzeller/rpi-rgb-led-matrix/tree/master/fonts) library.
