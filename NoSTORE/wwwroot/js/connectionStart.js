import { connection } from './signalConnection.js';

async function initConnection() {
    try {
        await connection.start();
    } catch (err) {
        console.error("Ошибка подключения SignalR: " + err);
    }
}

initConnection();