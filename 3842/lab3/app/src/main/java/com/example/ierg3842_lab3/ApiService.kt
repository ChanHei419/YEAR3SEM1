// 檔案路徑: app/src/main/java/com/example/ierg3842_lab3/ApiService.kt

package com.example.ierg3842_lab3

import com.google.gson.GsonBuilder
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.POST

// 這裡不再有 data class 的定義

interface MyApiService {
// 這裡的 LoginRequest 和 RegisterRequest 會自動引用 Datamodels.kt 中的定義
@POST("login/")
suspend fun postLogin(@Body request: LoginRequest): StatusResponse

@POST("register/")
suspend fun postRegister(@Body request: RegisterRequest): StatusResponse
}

object RetrofitInstance {
private const val BASE_URL = "http://10.0.2.2:53842/"

val api: MyApiService by lazy {
Retrofit.Builder()
.baseUrl(BASE_URL)
.addConverterFactory(GsonConverterFactory.create(GsonBuilder().create()))
.build()
.create(MyApiService::class.java)
}
}