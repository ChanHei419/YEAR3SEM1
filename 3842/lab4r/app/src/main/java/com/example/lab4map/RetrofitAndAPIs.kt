package com.example.lab4map


import com.google.gson.Gson
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*


// ============== API Request Models ==============
data class LoginRequest(val email: String, val password: String)

data class MessageItem(
    val roomId: String,
    val senderId: String,
    val senderName: String,
    val content: String
)

// ============== API Response Models ==============
data class ApiResponse<T>(
    val status: String,
    val message: String,
    val data: T? = null
)

data class LoginResponse(
    val status: String,
    val message: String,
    val userId: String = "",
    val name: String = ""
)

data class RegisterResponse(
    val status: String,
    val message: String,
    val userId: String = ""
)

data class ProfileResponse(
    val status: String,
    val profile: UserProfile? = null
)

data class MessageResponse(
    val status: String,
    val messages: List<MessageData> = emptyList()
)

data class UserProfile(
    val email: String,
    val name: String? = null,
    val phone: String? = null,
    val registrationDate: String? = null
)

data class MessageData(
    val senderId: String,
    val senderName: String,
    val content: String,
    val timestamp: String,
    val roomId: String
)

// ============== API Service Interface ==============
interface ApiService {
    // Authentication
    @POST("api/auth/register")
    suspend fun postRegister(@Body request: LoginRequest): LoginResponse

    @POST("api/auth/login")
    suspend fun postLogin(@Body request: LoginRequest): LoginResponse

    @GET("api/auth/profile/{user_id}")
    suspend fun getProfile(@Path("user_id") userId: String): ProfileResponse

    // Profile Management
    @PUT("api/profile/{user_id}")
    suspend fun updateProfile(
        @Path("user_id") userId: String,
        @Body profile: UserProfile
    ): ApiResponse<String>

    // Chat
    @GET("api/chat/messages/{room_id}")
    suspend fun getChatHistory(@Path("room_id") roomId: String): MessageResponse

    // Health Check
    @GET("/")
    suspend fun healthCheck(): ApiResponse<String>
}

// ============== Retrofit Singleton ==============
object RetrofitInstance {
    // ⚠️ IMPORTANT: Update this URL based on your setup
    // For Android Emulator: http://10.0.2.2:53842/
    // For Real Device: http://YOUR_COMPUTER_IP:53842/
    private val mainURL = "http://10.0.2.2:53842/"

    private val gson = Gson()

    private val retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(mainURL)
            .addConverterFactory(GsonConverterFactory.create(gson))
            .build()
    }

    val api: ApiService by lazy {
        retrofit.create(ApiService::class.java)
    }
}

// ============== WebSocket Configuration ==============
// For future WebSocket implementation
object WebSocketConfig {
    // Format: ws://10.0.2.2:53842/ws/chat/{room_id}
    // For Android Emulator
    fun getWebSocketUrl(roomId: String): String {
        return "ws://10.0.2.2:53842/ws/chat/$roomId"
    }

    // For real device, replace 10.0.2.2 with your computer IP
    fun getWebSocketUrlForDevice(computerIp: String, roomId: String): String {
        return "ws://$computerIp:53842/ws/chat/$roomId"
    }
}
