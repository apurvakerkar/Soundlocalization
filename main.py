from operator import truediv
import socket
import csv
import numpy as np
import pyaudio
import wave
import threading
import datetime

# IP address and port to listen on
HOST = '127.0.0.1'
PORT = 12346

trigger = 0

# Initialize socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)

# Accept incoming connection
print('Waiting for Unity connection...')
client_socket, addr = server_socket.accept()
print('Connected by', addr)

# Audio recording parameters
FORMAT = pyaudio.paInt16
CHANNELS = 7
RATE = 44100
CHUNK = 1024
RECORD_SECONDS = 5

# Initialize PyAudio
p = pyaudio.PyAudio()

# Initialize CSV file for writing Unity data
csv_filename = "unity_data2.csv"
csv_header = ["Time", "SoundFilePath", "Direction", "DirectionNormalized", "PositionOriginal", "PositionTarget"]

# Initialize audio file for writing audio data
audio_filename = "audio_recording2.wav"

# Define function to start audio recording
def start_audio_recording(temp):
    global trigger
    trigger = 1
    print("Starting audio recording...")
    

    stream = p.open(format=FORMAT,
                    channels=CHANNELS,
                    rate=RATE,
                    input=True,
                    frames_per_buffer=CHUNK)

    frames = []
    for i in range(0, int(RATE / CHUNK * RECORD_SECONDS)):
        data = stream.read(CHUNK)
        frames.append(data)

    stream.stop_stream()
    stream.close()
    p.terminate()

    wf = wave.open(audio_filename, 'wb')
    wf.setnchannels(CHANNELS)
    wf.setsampwidth(p.get_sample_size(FORMAT))
    wf.setframerate(RATE)
    wf.writeframes(b''.join(frames))
    wf.close()
    print("audio recording Done..")
    trigger = 0


audio_thread = threading.Thread(target = start_audio_recording, args = ("temp",))

# Set up CSV file for writing Unity data
with open(csv_filename, 'w', newline='') as csvfile:
    csv_writer = csv.writer(csvfile)
    csv_writer.writerow(csv_header)

    # Main loop to receive data from Unity
    while True:
        # Receive Unity data from client
        unity_data = client_socket.recv(1024).decode("utf-8")
        
        # print(f"{trigger}")
        if trigger:
         print(f"Received data from Unity: {unity_data}")
        
        
        if not unity_data:
            break

        # Start audio recording when requested
        if unity_data.strip() == "StartRecording":
            audio_thread.start()
            

        
        # Parse Unity data and write to CSV file
        # Here, you would parse and split the Unity data received and write it to the CSV file
        # For this example, let's just write a placeholder data
        unity_data_placeholder = ["00:00:00", "some/path/to/sound/file.wav", "X", "Y", "Z", "X", "Y", "Z"]
        csv_writer.writerow(unity_data_placeholder)

# Close the socket
client_socket.close()
server_socket.close()
