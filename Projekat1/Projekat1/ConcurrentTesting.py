import random
import threading
import requests

def send_request(url):
    try:
        response = requests.get(url)
        print(f"Request url {url}")
        print(response.text)
    except Exception as e:
        print(f"Error accessing {url}: {e}")

base_url = "http://127.0.0.1:10889/"
num_requests = 5
threads = []

words = ["sistemsko", "programiranje", "Elfak", "elfak", "semaphore", "Concurrency"]

for i in range(num_requests):
    url = f"{base_url}{'&'.join([word for word in [random.choice(words) for _ in range(random.randint(1, 5))]])}"
    thread = threading.Thread(target=send_request, args=(url,))
    threads.append(thread)
    thread.start()

for thread in threads:
    thread.join()
