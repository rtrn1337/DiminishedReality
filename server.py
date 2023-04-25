import time
import threading
import os 
import re
import glob
from PIL import Image
import argparse
import socket
import sys
import io
from io import BytesIO
from bin import predict
from omegaconf import OmegaConf
# For both Python 2.7 and Python 3.x
import hydra
import base64 
import subprocess
import torch
import time

# connect to google dns and get local network ip instead of localhost
def get_internal_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.connect(('8.8.8.8', 80))
    ip = s.getsockname()[0]
    s.close()
    return ip

_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
parser = argparse.ArgumentParser()
parser.add_argument("--port", default=5555, type=int, help="Port of the server")
args = parser.parse_args()
#ipaddress = "tcp://"+socket.gethostbyname(socket.gethostname())+":"+ str(args.port)

ipaddress = "tcp://"+get_internal_ip()+":"+ str(args.port)
print(">>>>> Starting Server - listening to ipaddress: {0} ".format(ipaddress))
_socket.bind((get_internal_ip(),args.port))
_socket.listen(1)
cwd = os.getcwd()
print(">>>>> Init NN") 
 
outpath = "/output_MATestImages"
@hydra.main(config_path='configs/prediction', config_name='default.yaml')
def main(predict_config: OmegaConf):
    print(">>>>> Starting Server") 
    predict_config.model.path = cwd+'/lama-places/lama-regular'
    predict_config.indir = cwd+'/fromdevice'
    predict_config.outdir = cwd+outpath
    predict.main(predict_config)
    print(">>>>> Starting Server - listening to ipaddress: {0} ".format(ipaddress))
    print(">>>>> Waiting for handshake...") 
main()
background = None
mask = None
handshake = False
message = ""
data = ""
prefix = ""
conn, addr = _socket.accept()


def recv_timeout(the_socket,buffsize,timeout=0.1):
    #make socket non blocking
    the_socket.setblocking(0)
    #total data partwise in an array
    total_data=[];
    data='';
    #beginning time
    begin=time.time()
    while True:
        #if you got some data, then break after timeout
        if total_data and time.time()-begin > timeout:
            break
        
        #if you got no data at all, wait a little longer, twice the timeout
        elif time.time()-begin > timeout*2:
            break
        
        #recv something
        try:
        	data = the_socket.recv(buffsize)
        	if data:
        		total_data.append(data.decode("utf-8"))
        		#change the beginning time for measurement
        		begin = time.time()
        	else:
        		#sleep for sometime to indicate a gap
        		time.sleep(0.0)
        		
        		break
        except KeyboardInterrupt:
            conn.close()
            sys.exit() 
        except Exception as e:
            #print(e)
            pass
    
    #join all parts to make final string
    return ''.join(total_data)


def ReadoutData():
    data = '';
    prefix = '';
    content = '';

    buffsize = 4096
    data = recv_timeout(conn,buffsize)
    if data == '':
        #print(">>>>> Waiting for data...")
        return '', '', False	
    else:
        #print('DATA##'+str(data)+'##ENDDATA')
        prefix = str(data).split(':')[0]
        content = str(data).split(':')[1]
        content = content.split('==')[0]
        content = content+'=='
        #print("...read data message content is"+content[0:30]+" ... " + content[-100:] + "with prefix " + prefix)
        return prefix, content, True	


while True:
    #  Wait for next request from client
    content = ""
    prefix = ""
    readoutdone = False;
    prefix, content, readoutdone = ReadoutData()

    if readoutdone:
        #print("What to do with prefix? Switch case to desired option: "+case)
        if prefix == 'handshake':  
            conn.send("handshake:Connection successfull_".encode("ascii"))
            print(">>>>> Handshake Done")
        elif prefix == 'background':
            background = Image.open(BytesIO(base64.b64decode(content)))
            background.save("fromdevice/background.jpg")
            conn.send("background:Background received successfull_".encode("ascii"))
            print(">>>>> Background Received") 
        elif prefix == 'mask':
            mask = Image.open(BytesIO(base64.b64decode(content)))
            mask.save("fromdevice/background_mask.png")
            conn.send("mask:Mask received successfull_".encode("ascii"))
            print(">>>>> Mask Received")
        elif prefix == 'close':
            print(">>>>> Unity Closed Restarting Server...") 
            os.execv(sys.executable, ['python'] + sys.argv)
    readoutdone = False;

    if(background is not None and mask is not None):
        background = None;
        mask = None;
        starttime = time.time();
        predict.doinpaint()
        endtime = time.time();
        print(endtime-starttime)
        for filename in glob.iglob('output/*.png', recursive=True):
            im = Image.open(filename, "r")
            #im.show()
            buffered = BytesIO()
            im.save(buffered, format="JPEG")
            image_bytes = buffered.getvalue()
            image = base64.b64encode(image_bytes)
            prefix = "inpaint:".encode("ascii")
            subfix = "_".encode("ascii")

            tosend = prefix+image+subfix
            conn.send(tosend)
