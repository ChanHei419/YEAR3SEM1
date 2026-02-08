# Problem 6
# 1155212799
# start cmd:  uvicorn P6_1155212799:app --host 0.0.0.0 --port 55726
# Important: please delete the docs in testing url for test if the final url contain it
# test at (delete docs in url if needed):  http://localhost:55726/docs
# Important: please delete the docs in testing url for test if the final url contain it
# I used AI to debug like I need declare the ModelData first as it shows error without declaration

from typing import Annotated
from fastapi import FastAPI, Form, Request
from fastapi.responses import JSONResponse, HTMLResponse
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel
class ModelData(BaseModel):
    height: float
    weight: float

app = FastAPI()
templates = Jinja2Templates(directory="templates")
@app.post("/model/")
async def login(height: float = Form(...), weight: float = Form(...)):
    # write your logic here
    
    if height == 0:
        response_content = {
            "result": -1.0,
            "error": "Height is zero"
        }
    else:
        bmi = weight / (height ** 2)
        response_content = {
            "result": bmi,
            "error": ""
        }
    return JSONResponse(content=response_content, status_code=200)

@app.get('/', response_class=HTMLResponse)
async def main(request: Request):
    return templates.TemplateResponse('P5_1155212799.html', {'request': request})  