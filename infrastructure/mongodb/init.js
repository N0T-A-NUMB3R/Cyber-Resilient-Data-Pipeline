db = db.getSiblingDB('cyber_logs');

db.createCollection('analysis_results');

db.analysis_results.createIndex({ eventId: 1 }, { unique: true });
db.analysis_results.createIndex({ analyzedAt: -1 });
db.analysis_results.createIndex({ threatLevel: 1 });
db.analysis_results.createIndex({ sourceIp: 1, analyzedAt: -1 });

print('MongoDB: database cyber_logs inizializzato.');
