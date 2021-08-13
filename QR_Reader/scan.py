import pyzbar.pyzbar as pyzbar	# 바코드(QR코드) 인식 관련 라이브리
import cv2 
import datetime
from PIL import ImageFont, ImageDraw, Image
import numpy as np		        # 행렬이나 배열을 수식처리하기위한 라이브러리
from typing import OrderedDict  # 순서가 지정된 딕셔너리
import paho.mqtt.client as mqtt # 데이터 전송을 위한 프로토콜
import json                     # 데이터 전송 타입

dev_id = 'QR_Reader'
broker_address = '210.112.19.50'    # 본인 주소넣기
pub_topic = 'QR_Reader/data/'

client2 = mqtt.Client(dev_id)
client2.connect(broker_address)

cap = cv2.VideoCapture(0)		# 웹캠 열기
# PK조 Fighting!!
#hashVal = 'D67C69FFACCF947DBEAD024F8FF722D0'
font = ImageFont.truetype('./fonts/NanumGothicBold.ttf', 20) # 글꼴파일을 불러옴

def send_data(name, phonenumber, gender):   # 윈도우에서 데이터를 받기 위한 함수설정
    visitdate = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
    #json data gen
    raw_data = OrderedDict()                # 순서지정 딕셔너리로 생성자 호출
    raw_data['Name'] = name                 # info[0]
    raw_data['PhoneNumber'] = phonenumber   # info[1]
    raw_data['Gender'] = gender             # info[2]
    raw_data['VisitDate'] = visitdate
    pub_data = json.dumps(raw_data, ensure_ascii=False, indent='\t') # 데이터 발행 설정   
    print(pub_data)
    #mqtt_publish
    client2.publish(pub_topic, pub_data)    # 토픽설정 후 발행(브로커와 연결된 후 실행되야함)

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
    # 저장이 실행되면 mqtt를 통해 데이터를 전송시킴
    elif key == ord('s'):   # s 입력시 저장
        print("{}".format(qrcode_data)) # 이미지 저장 설정
        cv2.imwrite('D:\\QR_{0}.jpg'.format(filedt), img)  

        info = qrcode_data.split("|")       # 데이터 핸들링을 위한 처리('|'기준으로 쪼갬)
        send_data(info[0],info[1],info[2])  # 배열을 통해 데이터별로 분리

        

cap.release()   # 웹캠 해제
cv2.destroyAllWindows() # 화면에 나타난 윈도우 종료(메모리 해제)