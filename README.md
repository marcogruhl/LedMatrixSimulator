# LED Matrix Simulator
This project should help to build new "Faces" for a LED Matrix without needing to connect directly to it. It uses a NamedPipeServer/Client to get data from the "real" project.

# Goals

- no need for connecting to actual matrix
- faster develpment
- hot reload
- debugging

# Future development

- sample client
- proxy to client
- proxy from client

# Why?

I am using it for C# development with the [rpi-rgb-led-matrix](https://github.com/hzeller/rpi-rgb-led-matrix/tree/master/fonts) library.