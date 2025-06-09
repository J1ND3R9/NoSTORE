package com.example.nostore

import android.content.Context

object TokenManager {
    private const val PREF_NAME = "auth_data"
    private const val KEY_TOKEN = "token"
    private const val KEY_USER_ID = "user_id"
    private const val KEY_NICKNAME = "nickname"

    private fun getSharedPreferences(context: Context) =
        context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE)

    fun saveToken(context: Context, token: String, userId: String) {
        with(getSharedPreferences(context).edit()) {
            putString(KEY_TOKEN, token)
            putString(KEY_USER_ID, userId)
            apply()
        }
    }

    fun saveUserInfo(context: Context, userInfo: UserInfo) {
        with(getSharedPreferences(context).edit()) {
            putString(KEY_NICKNAME, userInfo.nickname)
            apply()
        }
    }

    fun getAuthToken(context: Context): String? =
        getSharedPreferences(context).getString(KEY_TOKEN, null)

    fun getUserId(context: Context): String? =
        getSharedPreferences(context).getString(KEY_USER_ID, null)

    fun getNickname(context: Context): String? =
        getSharedPreferences(context).getString(KEY_NICKNAME, null)

    fun isLoggedIn(context: Context): Boolean =
        getAuthToken(context) != null

    fun clear(context: Context) {
        with(getSharedPreferences(context).edit()) {
            remove(KEY_TOKEN)
            remove(KEY_USER_ID)
            remove(KEY_NICKNAME)
            apply()
        }
    }
}