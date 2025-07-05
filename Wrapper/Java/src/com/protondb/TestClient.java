package com.protondb;

public class TestClient {
    public static void main(String[] args) {
        try {
            ProtonDBClient db = new ProtonDBClient("localhost", 9090, "admin123", "welcome");

            ProtonResponse insert = db.query("document.insert({ \"name\": \"Notebook\", \"price\": 10 })");
            System.out.println("Insert Response: " + insert);
            if (insert.isSuccess()) {
                System.out.println("[SUCCESS] Insert successful: " + insert.getMessage());
            } else {
                System.out.println("[ERROR] Insert failed: " + insert.getMessage());
            }

            ProtonResponse find = db.query("document.find({})");
            System.out.println("Find Response: " + find);
            if (find.isSuccess()) {
                System.out.println("[SUCCESS] Find successful: " + find.getMessage());
            } else {
                System.out.println("[ERROR] Find failed: " + find.getMessage());
            }

            db.close();
        } catch (Exception e) {
            System.err.println("[ERROR] Error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
