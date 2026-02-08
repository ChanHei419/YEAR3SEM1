plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)
}

android {
    namespace = "com.example.ierg3842_lab3"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.example.ierg3842_lab3"
        minSdk = 24
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    kotlinOptions {
        jvmTarget = "11"
    }
    buildFeatures {
        compose = true
    }
}

dependencies {

    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.activity.compose)
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.compose.ui)
    implementation(libs.androidx.compose.ui.graphics)
    implementation(libs.androidx.compose.ui.tooling.preview)
    implementation(libs.androidx.compose.material3)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    debugImplementation(libs.androidx.compose.ui.tooling)
    debugImplementation(libs.androidx.compose.ui.test.manifest)
    // Retrofit for networking
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")

// Coroutines for asynchronous tasks
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3") // 版本可能需要更新

// ViewModel and LiveData for UI state management
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.7.0")

// Navigation for moving between screens
    implementation("androidx.navigation:navigation-compose:2.7.7")

    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    // Gson converter for Retrofit
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    // ViewModel for Compose
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.7.0")
    // LiveData (雖然我們主要用 State，但這是好習慣)
    implementation("androidx.lifecycle:lifecycle-livedata-ktx:2.7.0")
    // Coroutines
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")
    // Material Icons
    implementation("androidx.compose.material:material-icons-extended:1.6.3")
    // Date Picker
    implementation("androidx.compose.material3:material3:1.2.1")
}