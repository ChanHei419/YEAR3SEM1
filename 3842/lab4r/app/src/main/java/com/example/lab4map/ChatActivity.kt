package com.example.lab4map
import java.time.LocalDateTime

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.lab4map.ui.theme.Lab4mapTheme
import kotlinx.coroutines.launch

// ============== Chat Data Models ==============
data class ChatMessage(
    val senderId: String,
    val senderName: String,
    val content: String,
    val timestamp: String = ""
)

// ============== Chat ViewModel ==============
class ChatViewModel : ViewModel() {
    var messages by mutableStateOf<List<ChatMessage>>(emptyList())
    var isLoading by mutableStateOf(true)
    var errorMessage by mutableStateOf("")
    var roomId by mutableStateOf("")

    fun loadChatHistory(roomId: String) {
        this.roomId = roomId
        viewModelScope.launch {
            try {
                val response = RetrofitInstance.api.getChatHistory(roomId)
                if (response.status == "OK") {
                    messages = response.messages.map { msg ->
                        ChatMessage(
                            senderId = msg.senderId,
                            senderName = msg.senderName,
                            content = msg.content,
                            timestamp = msg.timestamp
                        )
                    }
                    errorMessage = ""
                } else {
                    errorMessage = "Failed to load messages"
                }
                isLoading = false
            } catch (e: Exception) {
                errorMessage = "Error: ${e.message}"
                isLoading = false
            }
        }
    }

    fun sendMessage(senderId: String, senderName: String, content: String) {
        if (content.isBlank()) return

        // Add message locally first (optimistic update)
        val newMessage = ChatMessage(
            senderId = senderId,
            senderName = senderName,
            content = content,
            timestamp = java.lang.System.currentTimeMillis().toString()
        )
        messages = messages + newMessage

        // Send to server
        viewModelScope.launch {
            try {
                val messageItem = MessageItem(
                    roomId = roomId,
                    senderId = senderId,
                    senderName = senderName,
                    content = content
                )
                // In production, send via WebSocket
                // For now, messages are saved when sent
            } catch (e: Exception) {
                errorMessage = "Failed to send message: ${e.message}"
            }
        }
    }
}

// ============== Chat Activity ==============
class ChatActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Lab4mapTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    ChatScreen()
                }
            }
        }
    }
}

@Composable
fun ChatScreen() {
    val context = LocalContext.current
    val viewModel = remember { ChatViewModel() }
    val userId = remember { (context as? ChatActivity)?.intent?.getStringExtra("userId") ?: "guest" }
    val shopName = remember { (context as? ChatActivity)?.intent?.getStringExtra("shopName") ?: "Pizza Hut" }
    val roomId = remember { "$userId-$shopName-room" }

    var messageText by remember { mutableStateOf("") }

    // Load chat history on first composition
    LaunchedEffect(Unit) {
        viewModel.loadChatHistory(roomId)
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(0.dp)
    ) {
        // Header
        Surface(
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
            color = Color(0xFF2080B2),
            shadowElevation = 4.dp
        ) {
            Row(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(horizontal = 16.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                IconButton(onClick = {
                    (context as? ChatActivity)?.finish()
                }) {
                    Icon(Icons.Default.ArrowBack, contentDescription = "Back", tint = Color.White)
                }

                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = "Chat with $shopName",
                        fontSize = 18.sp,
                        fontWeight = FontWeight.Bold,
                        color = Color.White
                    )
                    Text(
                        text = "Room: $roomId",
                        fontSize = 10.sp,
                        color = Color.White.copy(alpha = 0.7f)
                    )
                }
            }
        }

        // Messages Area
        if (viewModel.isLoading) {
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(12.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
                reverseLayout = false
            ) {
                items(viewModel.messages) { message ->
                    ChatMessageBubble(
                        message = message,
                        isCurrentUser = message.senderId == userId
                    )
                }
            }
        }

        // Error Message
        if (viewModel.errorMessage.isNotEmpty()) {
            Surface(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp),
                color = Color(0xFFFFCDD2),
                shape = RoundedCornerShape(4.dp)
            ) {
                Text(
                    text = viewModel.errorMessage,
                    modifier = Modifier.padding(12.dp),
                    fontSize = 12.sp,
                    color = Color(0xFFC62828)
                )
            }
        }

        // Input Area
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .background(Color(0xFFF5F5F5))
                .padding(8.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            OutlinedTextField(
                value = messageText,
                onValueChange = { messageText = it },
                placeholder = { Text("Type a message...") },
                modifier = Modifier
                    .weight(1f)
                    .height(48.dp),
                singleLine = true,
                shape = RoundedCornerShape(12.dp)
            )

            IconButton(
                onClick = {
                    if (messageText.isNotBlank()) {
                        viewModel.sendMessage(userId, "User-$userId", messageText)
                        messageText = ""
                    }
                },
                modifier = Modifier
                    .size(48.dp)
                    .background(Color(0xFF2080B2), RoundedCornerShape(12.dp))
            ) {
                Icon(
                    Icons.Default.Send,
                    contentDescription = "Send",
                    tint = Color.White
                )
            }
        }
    }
}

@Composable
fun ChatMessageBubble(message: ChatMessage, isCurrentUser: Boolean) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        horizontalArrangement = if (isCurrentUser) Arrangement.End else Arrangement.Start
    ) {
        Surface(
            modifier = Modifier
                .widthIn(max = 280.dp),
            color = if (isCurrentUser) Color(0xFF2080B2) else Color(0xFFE8F5E9),
            shape = RoundedCornerShape(
                topStart = 12.dp,
                topEnd = 12.dp,
                bottomStart = if (isCurrentUser) 12.dp else 0.dp,
                bottomEnd = if (isCurrentUser) 0.dp else 12.dp
            )
        ) {
            Column(
                modifier = Modifier.padding(12.dp)
            ) {
                Text(
                    text = message.senderName,
                    fontSize = 10.sp,
                    fontWeight = FontWeight.Bold,
                    color = if (isCurrentUser) Color.White else Color(0xFF2E7D32)
                )
                Text(
                    text = message.content,
                    fontSize = 14.sp,
                    color = if (isCurrentUser) Color.White else Color.Black,
                    modifier = Modifier.padding(top = 4.dp)
                )
                Text(
                    text = message.timestamp.take(16),
                    fontSize = 8.sp,
                    color = if (isCurrentUser) Color.White.copy(alpha = 0.7f) else Color.Gray,
                    modifier = Modifier.padding(top = 4.dp)
                )
            }
        }
    }
}
