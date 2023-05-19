# LED Matrix Simulator
This project should help to build new "Faces" for a LED Matrix without needing to connect directly to it. It uses a NamedPipeServer/Client to get data from the "real" project.

![LedMatrixSimulator](https://github.com/marcogruhl/LedMatrixSimulator/assets/9695930/c4f6f484-fb31-42d4-8319-c045c33f71d8)

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
