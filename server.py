#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

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
#p = os.system("bin/predict.py model.path=$(pwd)/lama-places/lama-fourier indir=$(pwd)/fromdevice outdir=$(pwd)/output_MATestImages")
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

	# try:
	# 	prefix = data.decode("utf-8").split(':')[0]
	# 	message = data.decode("utf-8").split(':')[1]
	# 	print("...read data rawmessage is  - message content is with prefix {2}.".format(data.decode("utf-8"),message,prefix))
	# 	return prefix, message, True
	# except IndexError:
	# 	print("indexerror " + data.decode("utf-8"))
	# 	return prefix, message, False
	# 	#break;



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
            #print('CONTENT##'+str(content)+'##ENDCONTENT')
            background = Image.open(BytesIO(base64.b64decode(content)))
            #background = Image.open(BytesIO(base64.b64decode(str(content))))
            #background.show()
            background.save("fromdevice/background.jpg")
            conn.send("background:Background received successfull_".encode("ascii"))
            print(">>>>> Background Received") 
        elif prefix == 'mask':
            mask = Image.open(BytesIO(base64.b64decode(content)))
            #mask.show()
            mask.save("fromdevice/background_mask.png")
            conn.send("mask:Mask received successfull_".encode("ascii"))
            print(">>>>> Mask Received")
        elif prefix == 'close':
            print(">>>>> Unity Closed Restarting Server...") 
            os.execv(sys.executable, ['python'] + sys.argv)
    readoutdone = False;
    #case = message[:10]
    # if not handshake:
    # 	handshake = True;
    # 	print("Client Connected with message {0}".format(message))
    # 	conn.send(b"SERVER: connection established")
    # 	message = "";
    # print("Waiting for background...")
    # if background is None and message is not None:
    # 	#print("Received Backgroundrequest %s" % message)
    # 	# image converted as string
    # 	background = Image.open(BytesIO(base64.b64decode(message)))
    # 	print("Received request: %s" % background)
    # 	conn.send(b"SERVER Received request: background")
    # 	message = "";
    # 	#background.show()
    #  	#background.save("fromdevice/background.png") 
    # print("Waiting for Mask...")
    # #elif mask is None and message is not None:
     #   image_data = message # byte values of the image
      #  mask = Image.open(io.BytesIO(image_data))
       # mask.show()
        #mask.save("fromdevice/background_mask.png")
        #print("Received request: %s" % mask)
        #_socket.send(b"Received request: mask")
    #elif callforImage is None and message is not None:
    #	callforImage = message;

    if(background is not None and mask is not None):
        background = None;
        mask = None;
        starttime = time.time();
        predict.doinpaint()
        endtime = time.time();
        print(endtime-starttime)
        for filename in glob.iglob('output_MATestImages/*.png', recursive=True):
        #for filename in glob.iglob('fromdevice/background.png', recursive=True):
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
            #print("Send Result - start waiting again..." +tosend.decode("ascii")[-50:])
 
    #  Do some 'work'.
    #  Try reducing sleep time to 0.01 to see how blazingly fast it communicates
    #  In the real world usage, you just need to replace time.sleep() with
    #  whatever work you want python to do, maybe a machine learning task?
    # time.sleep(1)
    #  Send reply back to client
    #  In the real world usage, after you finish your work, send your output here
 
    #socket.send(b"Received")
