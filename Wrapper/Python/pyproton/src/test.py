import subprocess as sp
import time

process  = sp.Popen("D:\\STUDYYY\\ProtonDB\\ProtonDB.exe",stdin = sp.PIPE , stdout=sp.PIPE , stderr=sp.PIPE ,text=True,shell=True)

process.stdin.write('db.create("wathaaaaa4")')

process.stdin.flush()

time.sleep(2)

for _ in range(10):
    line = process.stdout.readline()
    if not line:
        break
    print("OUTPUT:", line.strip())







