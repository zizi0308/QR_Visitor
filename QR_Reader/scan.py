import pyzbar.pyzbar as pyzbar	# 바코드(QR코드) 인식 관련 라이브리
import cv2 
import datetime
from PIL import ImageFont, ImageDraw, Image
import numpy as np		        # 행렬이나 배열을 수식처리하기위한 라이브러리
from typing import OrderedDict
import paho.mqtt.client as mqtt
import json

dev_id = 'QR_Reader'
broker_address = '210.112.19.50'    #본인 주소넣기
pub_topic = 'QR_Reader/data/'

client2 = mqtt.Client(dev_id)
client2.connect(broker_address)
# print('MQTT Client Connected')  # 이 메세지가 찍혀야 접속되는 것

cap = cv2.VideoCapture(0)		# 웹캠 열기
# PK조 Fighting!!
#hashVal = 'D67C69FFACCF947DBEAD024F8FF722D0'
font = ImageFont.truetype('./fonts/NanumGothicBold.ttf', 20) # 글꼴파일을 불러옴

def send_data(name, phonenumber, gender):
    visitdate = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    #json data gen
    raw_data = OrderedDict()    
    raw_data['Name'] = name
    raw_data['PhoneNumber'] = phonenumber
    raw_data['Gender'] = gender
    raw_data['VisitDate'] = visitdate
    pub_data = json.dumps(raw_data, ensure_ascii=False, indent='\t')
    print(pub_data)
    #mqtt_publish
    client2.publish(pub_topic, pub_data)

i = 0
while (cap.isOpened()):         # 웹캠이 실행되는 동안
    ret, img = cap.read()       # 카메라 현재 영상 로드 img에 저장, ret T/F

    # 현재시간 추가를 위한 코드
    now = datetime.datetime.now()
    filedt = now.strftime('%Y%m%d_%H%M%S')
    
    if not ret:                 # return이 False면 continue
        continue

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)    # 이미지 흑백으로 인식, 데이터 전처리1

    decoded = pyzbar.decode(gray)
    qrcode_data = ''            # 데이터값 초기화

    for d in decoded:           # 큐알코드를 받은다음
        x, y, w, h = d.rect     # 직사각형으로 이미지를 정렬, 데이터 전처리2
        
        # QR코드 에서 정보를 해독
        qrcode_data = d.data.decode("utf-8")    
        qr_type = d.type    

        img_pil = Image.fromarray(img)  # numpy 배열을 pil이미지로 변환
        draw = ImageDraw.Draw(img_pil)  # 위의 것으로 draw작업을 위한 코드(빨간박스 넣기위한 이미지화)
        
        cv2.rectangle(img, (x, y), (x + w, y + h), (0, 0, 255), 2)  # 직사각형을 그려 QR코드 감지 확인
        
        # QR코드에서 받은 정보를 다시 인코딩해 텍스트로 내보냄
        text = '%s (%s)' % (str(qrcode_data).encode('utf-8').decode('utf-8'), qr_type)  
        draw.text((0, 0),  text, font=font, fill=(255,255,255,0))

        img = np.array(img_pil)     # 이미지 객체를 다시 numpy 배열로 변환
        cv2.rectangle(img, (x, y), (x + w, y + h), (0, 0, 255), 2)  # 큐알코드 인식 시 빨간 네모박스 생성
        # print("{}".format(qrcode_data))
        # cv2.putText(img, text, (x, y), cv2.FONT_HERSHEY_SIMPLEX,
        #             1, (0, 255, 255), 2, cv2.LINE_AA)

    cv2.imshow('QR_Reader', img)  # 로드한 영상을 창에 띄움

    key = cv2.waitKey(1)
    if key == ord('q'): # q 입력시 종료
        break
    elif key == ord('s'):   # s 입력시 저장
        print("{}".format(qrcode_data)) # 이미지 저장 설정
        cv2.imwrite('D:\\QR_{0}.jpg'.format(filedt), img)  

        info = qrcode_data.split("|")
        send_data(info[0],info[1],info[2])

        

cap.release()   # 웹캠 해제
cv2.destroyAllWindows() # 화면에 나타난 윈도우 종료(메모리 해제)