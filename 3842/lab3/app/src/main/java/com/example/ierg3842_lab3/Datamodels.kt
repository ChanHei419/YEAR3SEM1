package com.example.ierg3842_lab3

import com.google.gson.annotations.SerializedName
data class RegisterRequest(
@SerializedName("email") val email: String,
@SerializedName("password") val password: String,
@SerializedName("district") val district: String? = null
)
data class LoginRequest(
@SerializedName("email") val email: String,
@SerializedName("password") val password: String
)
data class StatusResponse(
@SerializedName("status") val status: String,
@SerializedName("message") val message: String
)