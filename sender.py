import cv2
import json
import socket

HOST = "127.0.0.1"
PORT = 50007

def detect_person(image_path: str) -> bool:
    # OpenCVの歩行者検出器
    hog = cv2.HOGDescriptor()
    hog.setSVMDetector(cv2.HOGDescriptor_getDefaultPeopleDetector())

    img = cv2.imread(image_path)
    if img is None:
        print("画像が見つかりません")
        return False

    rects, _ = hog.detectMultiScale(img, winStride=(8, 8), padding=(8, 8), scale=1.05)
    return len(rects) > 0

if __name__ == "__main__":
    # サンプル画像ファイル (自分で適当な人物写真を置く)
    image_path = "sample.jpg"

    detected = detect_person(image_path)
    payload = {"person_detected": detected}
    message = json.dumps(payload) + "\n"

    # Unityに送信
    with socket.create_connection((HOST, PORT)) as sock:
        sock.sendall(message.encode("utf-8"))

    print("送信しました:", payload)