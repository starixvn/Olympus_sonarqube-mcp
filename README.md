# Cursor MCP Server

MCP server cho Cursor AI để tương tác với SonarQube.

## Yêu cầu

- .NET 9
- SonarQube Server
- SonarQube Token

## Cài đặt

1. Clone repository
2. Cập nhật cấu hình SonarQube trong `appsettings.json`:
   ```json
   {
     "SonarQube": {
       "BaseUrl": "http://your-sonarqube-server",
       "Token": "your-sonarqube-token"
     }
   }
   ```
3. Chạy ứng dụng:
   ```bash
   dotnet run --project CursorMCP.Api
   ```

## API Endpoints

### Lấy danh sách issues của project
```
GET /api/sonarqube/projects/{projectKey}/issues
```

### Lấy chi tiết issue
```
GET /api/sonarqube/issues/{issueKey}
```

### Lấy gợi ý sửa lỗi
```
GET /api/sonarqube/issues/{issueKey}/fix-suggestion
```

## Swagger UI

Swagger UI có sẵn tại `/swagger` khi chạy ứng dụng trong môi trường Development. 