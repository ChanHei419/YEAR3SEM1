#mongodb+srv://cccheilllun419:1155212799@cluster1155.mdxxcaj.mongodb.net/?retryWrites=true&w=majority&appName=Cluster1155
# 檔案路徑: C:\Github\3842\lab3\main.py
# 檔案路徑: C:\Github\3842\lab3\main.py

from fastapi import FastAPI
from pydantic import BaseModel, EmailStr, Field
from pymongo.mongo_client import MongoClient
from pymongo.server_api import ServerApi
import certifi

# --- FastAPI App 初始化 ---
app = FastAPI()

# --- MongoDB 連接 ---
# *** 重要：請務必將 <ID>, <PASSWORD>, <CLUSTERNAME> 換成你自己的 MongoDB Atlas 資訊 ***
uri = "mongodb+srv://cccheilllun419:1155212799@cluster1155.mdxxcaj.mongodb.net/?retryWrites=true&w=majority&appName=Cluster1155"

try:
    # 使用 certifi 來處理 SSL 憑證
    client = MongoClient(uri, server_api=ServerApi('1'), tlsCAFile=certifi.where())
    # 發送一個 ping 命令來確認連接是否成功
    client.admin.command('ping')
    print("成功連接到 MongoDB！")
except Exception as e:
    print(f"連接 MongoDB 失敗: {e}")
    # 如果連接失敗，伺服器啟動就沒有意義，直接退出
    exit()

# 選擇你的數據庫和集合
db = client["IERG3842_Lab3"]
collection = db["users"]

# --- Pydantic Models (數據模型) ---
class StatusResponse(BaseModel):
    status: str
    message: str

# 登入請求的模型
class LoginItem(BaseModel):
    email: EmailStr
    password: str

# 註冊請求的模型
class RegisterItem(BaseModel):
    email: EmailStr
    password: str = Field(..., min_length=8, max_length=20)
    district: str | None = None

# --- API Endpoints ---

@app.post("/login/")
async def login_logic(item: LoginItem):
    # 1. 根據 email 查找用戶
    user_in_db = collection.find_one({"email": item.email})

    # 2. 如果找不到用戶
    if not user_in_db:
        print("User does not exist")
        return {"status": "ERROR", "message": "User does not exist"}

    # 3. 如果找到了用戶，但密碼不匹配
    if user_in_db["password"] != item.password:
        print("Incorrect password")
        return {"status": "ERROR", "message": "Incorrect password"}

    # 4. 如果用戶存在且密碼正確，登入成功
    print(f"User {item.email} login successful")
    return {"status": "OK", "message": f"User {item.email} login successful"}


@app.post("/register/")
async def register_logic(item: RegisterItem):
    # 1. 檢查 email 是否已經被註冊
    if collection.find_one({"email": item.email}):
        print("Account is already created")
        return {"status": "ERROR", "message": "Account is already created"}

    # 2. 如果 email 未被註冊，將新用戶資料存入數據庫
    user_data = item.model_dump(exclude_unset=True)
    collection.insert_one(user_data)

    # 3. 返回成功訊息
    print(f"User {item.email} is created")
    return {"status": "OK", "message": f"User {item.email} is created"}


@app.get("/")
async def root():
    return {"message": "IERG3842 Lab 3 Server is running!"}