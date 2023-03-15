import json
import win32pipe
import win32file

class PipeClient:

  def __init__(self, name: str) -> None:
    self.handle = win32file.CreateFile(
      f"\\\\.\\pipe\\{name}", win32file.GENERIC_READ | win32file.GENERIC_WRITE, 
      0, None, win32file.OPEN_EXISTING, 0, None
    )
    win32pipe.SetNamedPipeHandleState(self.handle, win32pipe.PIPE_READMODE_BYTE, None, None)

  def __del__(self) -> None:
    if self.handle:
      win32file.CloseHandle(self.handle)
  
  def __enter__(self):
    return self
  
  def __exit__(self, ex_type, ex_value, trace):
    self.__del__()
  
  def write(self, data) -> None:
    win32file.WriteFile(self.handle, json.dumps(data).encode('utf-8'))
  
  def read(self) -> bytes:
    _, buf = win32file.ReadFile(self.handle, 4)
    size_to_read = int.from_bytes(bytes(buf), 'little')
    _, buf = win32file.ReadFile(self.handle, size_to_read)
    return bytes(buf)
