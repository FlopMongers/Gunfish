import socket

def start_server():
    server_socket = socket.socket()         
    server_socket.bind(('0.0.0.0', 8080)) # Listen on all IPs, port 8080
    server_socket.listen(0) # Allow any amount of backlog connections

    client_socket, client_address = server_socket.accept()
    print(f"Connection from {client_address} has been established.")

    while True:
        data = client_socket.recv(1024).decode() # Receive and decode data from client
        if data:
            print(f"Received data: {data}")

start_server()
