# 🔐 filelink-oauth-sandbox

**filelink-oauth-sandbox** は、  
**Keycloak + OAuth2.0 認証** を用いてファイルを安全に送受信する  
**サンドボックス（実験環境）** です。

This project demonstrates how to build a **secure file upload API**  
protected by **OAuth2.0 authentication via Keycloak**.  
It includes both the **API provider** and **client app** for testing.

---

## 🧩 Features / 機能

- ✅ OAuth2.0 (Client Credentials Flow) 認証対応  
- ✅ Keycloak を Docker で簡単起動  
- ✅ 認証必須のファイルアップロードAPI  
- ✅ クライアントアプリがトークンを取得して送信  
- ✅ ローカルストレージへのファイル保存  

---

## 🧱 Architecture / 構成図

```
+----------------------+ +----------------------+
| File Sender (Client) | ---> | File API Server |
|----------------------| |----------------------|
| - OAuth2 認証         || - Keycloakで検証    |
| - ファイル送信(POST)   || - ファイル受信/保存  |
| - トークン取得        | |                     |
|                      |  |                     |
+----------------------+ +----------------------+
                         │
                         ▼
                +------------------+
                | Local Storage    |
                | (storage folder) |
                +------------------+
```

---

## 📁 Directory Structure / フォルダ構成

```
filelink-oauth-sandbox/
├─ docker-compose.yml      # Keycloak 起動設定
├─ keycloak_data/          # Keycloak 永続化データ
│
├─ server/                 # ファイル受信API (.NET 8)
│  ├─ Program.cs
│  └─ storage/             # アップロード先
│
├─ client/                 # ファイル送信クライアント (.NET 8)
│  └─ Program.cs
│
└─ README.md
```

---

## ⚙️ Setup / セットアップ手順

### 1️⃣ Run Keycloak (Docker)

```bash
docker-compose up -d
```

- アクセス → http://localhost:8080
- ログイン → admin / admin

### 2️⃣ Configure Keycloak

- **Realm**: sandbox
- **Client**: filelinkage-client
- **Access Type**: Confidential
- **Client Authentication**: ✅ ON
- **Redirect URI**: http://localhost:5000/*
- **Client Secret**: （控えておく）

**Token endpoint**:
```
http://localhost:8080/realms/sandbox/protocol/openid-connect/token
```

### 3️⃣ Run API Server

```bash
cd server
dotnet run
```

- Swagger UI → http://localhost:5000/swagger

### 4️⃣ Run Client

`client/Program.cs` に取得した `client_secret` を設定し、実行：

```bash
cd client
dotnet run
```

**出力例**:
```json
{
  "status": "success",
  "fileName": "test.txt",
  "storedPath": "C:\\...\\storage\\test.txt"
}
```
---

## 🔐 OAuth2 Flow (Client Credentials)

1. Client requests token from Keycloak
2. Keycloak issues an access token
3. Client sends the token in Authorization: Bearer header
4. API verifies JWT via Keycloak's public key
5. File upload proceeds if token is valid

---

## 🧪 Example API

### Request

**POST** `/api/files/upload`

### Header

```
Authorization: Bearer {access_token}
Content-Type: multipart/form-data
```

Body

```
file=@example.txt
```

Response

```json
{
  "status": "success",
  "fileName": "example.txt",
  "storedPath": "/storage/example.txt"
}
```

---

## 🧰 Future Ideas / 今後の拡張案

- 🔄 Authorization Code Flow（ユーザーログイン対応）
- ☁️ S3 / Azure Blob ストレージ対応
- 🪣 ファイルメタ情報をDB保存
- 🧾 トークンログ・監査ログ出力
- 🔧 APIとClientのDocker Compose統合

---

## 🧑‍💻 Author

Created by Kazuhiro Nishina  
For experimental use and learning of Keycloak & OAuth2 integration.

---

## 📄 License

MIT License