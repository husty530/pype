import sys
import numpy as np
from pype import *

if __name__ == '__main__':
  with PipeClient(sys.argv[1]) as pipe:
    w = int(sys.argv[2])
    h = int(sys.argv[3])
    while True:
      buf = pipe.read()
      frame = np.frombuffer(buf, dtype=np.uint8).reshape(h, w, 3)
      # run DNN inference
      # get box or polygon with label
      # write list of dict like this:
      dummy_box1 = { 'label': "person", 'coordinates': [ [100, 100], [200, 100], [200, 100], [200, 200] ] }
      dummy_box2 = { 'label': "dog",    'coordinates': [ [450, 300], [500, 300], [450, 450], [500, 450] ] }
      pipe.write([dummy_box1, dummy_box2])