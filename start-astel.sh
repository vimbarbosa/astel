#!/bin/bash
set -e

# ===============================
# 🧠 Variáveis de diretório
# ===============================
API_PATH="/mnt/c/manus/astel/src/ASTEL.Api/ASTEL.Api"
FRONT_PATH="/mnt/c/manus/astel-ui/app"
API_CONTAINER_NAME="astel-api"
FRONT_CONTAINER_NAME="astel-frontend"

# ===============================
# 🧹 Limpa containers antigos
# ===============================
echo "🧹 Limpando containers antigos..."
docker rm -f $API_CONTAINER_NAME $FRONT_CONTAINER_NAME 2>/dev/null || true

# ===============================
# 🏗️ Build da API
# ===============================
echo "🚀 Fazendo build da API..."
cd "$API_PATH"
docker compose build
docker compose up -d

# ===============================
# 🌐 Build do Frontend
# ===============================
echo "🌈 Fazendo build do frontend..."
cd "$FRONT_PATH"

docker build -t $FRONT_CONTAINER_NAME .

# Roda o frontend servindo na porta 5173
docker run -d -p 5173:80 --name $FRONT_CONTAINER_NAME $FRONT_CONTAINER_NAME

# ===============================
# ✅ Status final
# ===============================
echo ""
echo "🎉 ASTEL está rodando!"
echo "------------------------------------"
echo "🌍 Frontend: http://localhost:5173"
echo "⚙️  Backend : http://localhost:5000"
echo "------------------------------------"
echo ""
docker ps --format "table {{.Names}}\t{{.Ports}}\t{{.Status}}"
