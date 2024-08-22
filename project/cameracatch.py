import cv2
import os

username = "your_username"
password = "your_password"
nvr_ip = "your_nvr_ip"
port = "your_port"

output_image_path = r"E:\Testcam\captured_image.jpg"

rtsp_url = f"rtsp://{username}:{password}@{nvr_ip}:{port}/"
print(rtsp_url)

video = cv2.VideoCapture(0)


def capture():
    if ret:
        cv2.imwrite(output_image_path, frame)
        print(f"Capture has been save at {output_image_path}")
    else:
        print("Can't not capture!")

if not video.isOpened():
    print("Can't open camera!")
    exit()


ret, frame = video.read()

capture()

video.release()
cv2.destroyAllWindows()
