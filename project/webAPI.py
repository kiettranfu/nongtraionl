const express = require('express');
const fs = require('fs');

const app = express();
const port = 3000;

app.use(express.json()); // Để parse JSON từ body của request

// API GET: Lấy toàn bộ dữ liệu
app.get('/data', (req, res) => {
    fs.readFile('data.json', 'utf8', (err, data) => {
        if (err) {
            return res.status(500).json({ message: 'Lỗi server' });
        }
        res.json(JSON.parse(data));
    });
});

// API POST: Thêm dữ liệu mới
app.post('/data', (req, res) => {
    const { RFID, data_1, data_2 } = req.body;

    if (!RFID || !data_1 || !data_2) {
        return res.status(400).json({ message: 'Thiếu dữ liệu đầu vào' });
    }

    fs.readFile('data.json', 'utf8', (err, data) => {
        if (err) {
            return res.status(500).json({ message: 'Lỗi server' });
        }

        const currentData = JSON.parse(data);
        const newData = { RFID, data_1, data_2 };
        currentData.push(newData);

        fs.writeFile('data.json', JSON.stringify(currentData, null, 2), (err) => {
            if (err) {
                return res.status(500).json({ message: 'Lỗi khi ghi dữ liệu' });
            }
            res.status(201).json({ message: 'Thêm dữ liệu thành công', data: newData });
        });
    });
});

// Chạy server
app.listen(port, () => {
    console.log(Server đang chạy tại http://localhost:${port});
});
