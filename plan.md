# ERMSystem Master Plan - Private Hospital Platform

## 1) Muc tieu

Xay dung ERMSystem thanh he thong quan ly benh vien tu day du chuc nang thuc te, kien truc mo rong duoc, bao mat cao, dam bao hieu nang va kha nang van hanh on dinh.

Muc tieu ky thuat bat buoc:

- Microservice architecture (theo bounded context, khong share database truc tiep).
- Redis (cache, distributed lock, rate limit, token blacklist/session cache).
- JWT access token + refresh token (rotation + revoke).
- RabbitMQ cho event-driven messaging (notification, integration async, workflow).
- Observability day du (logs, metrics, tracing, alerting).
- CI/CD + containerized deployment.

## 2) Kien truc tong the de xay dung

## 2.1 Kieu kien truc

- Giai doan 1: Modular Monolith chuan hoa boundaries trong code hien tai.
- Giai doan 2: Tach dan thanh Microservices theo domain on dinh.

## 2.2 De xuat Microservices

1. API Gateway/BFF
- Entry point cho Web/Mobile.
- Route, auth gateway policy, rate limiting, request aggregation.

2. Identity & Access Service
- Dang ky/dang nhap, JWT access token, refresh token, RBAC/permission.
- Quan ly session, revoke token, audit login.

3. Patient Service
- Ho so benh nhan, thong tin hanh chinh, lien he, bao hiem, lich su co ban.

4. Doctor/Staff Service
- Ho so bac si, lich lam viec, khoa/phong ban, phan quyen nhan su.

5. Appointment Service
- Dat lich, doi lich, huy lich, check-in, queue.

6. EMR Service (Medical Record)
- Benh an dien tu, chan doan, ket qua kham, tai lieu y te.

7. Prescription & Pharmacy Service
- Ke don, cap nhat don thuoc, ton kho thuoc co ban, canh bao tuong tac.

8. Billing & Payment Service
- Bang gia, hoa don, thanh toan, cong no, doi soat.

9. Notification Service
- Gui tin nhan SMS/Email/Zalo/App notification.
- Nhan message tu RabbitMQ va xu ly retry/dead-letter.

10. Reporting & Analytics Service
- Bao cao van hanh, tai chinh, KPI phong kham/benh vien.

11. Integration Service
- Ket noi ben thu 3: payment gateway, insurance, LIS/PACS (neu co).

## 2.3 Ha tang chung

- Service discovery/config: co the dung static + env giai doan dau, nang cap sau.
- Redis cluster/sentinel cho cache/session/lock.
- RabbitMQ + DLX/DLQ + retry policy.
- Object storage cho tai lieu (don thuoc scan, anh, ket qua can lam sang).
- SQL Server/PostgreSQL moi service 1 schema/database rieng (khong join cross-service truc tiep).

## 3) Functional scope (day du nghiep vu)

## 3.1 Core hospital operations

- Quan ly benh nhan: tao/sua/tim kiem/gop ho so, lich su kham.
- Quan ly bac si/nhan vien: profile, chuyen khoa, lich lam.
- Quan ly lich hen: dat lich online/offline, check-in, queue token.
- Kham benh: tiep nhan, trieu chung, chan doan, chi dinh.
- Benh an dien tu (EMR): tao/sua/ky duyet, lich su thay doi.
- Ke don thuoc: tao don, phat hanh, doi soat.
- Vien phi: tam tinh, chot hoa don, thanh toan nhieu kenh.
- Thong bao: nhac lich, ket qua, thong bao thanh toan, thong bao noi bo.
- Dashboard: luong benh nhan, doanh thu, ti le tai kham, load bac si.

## 3.2 Advanced modules

- Quan ly khoa/phong/giuong (neu mo rong inpatient).
- Dich vu can lam sang: xet nghiem, chan doan hinh anh.
- Lich su toa thuoc va canh bao tuong tac/co chong chi dinh.
- Quan ly tai lieu y te va mau bieu.
- Lich su audit nghiep vu (ai sua gi, luc nao).

## 3.3 CRM/Patient engagement

- Nhat ky cham soc khach hang.
- Chien dich nhac tai kham/tiem chung.
- Danh gia hai long sau kham.

## 4) Security & compliance requirements

- JWT access token ngan han (10-30 phut) + refresh token dai han (7-30 ngay).
- Refresh token rotation: moi lan refresh phai cap token moi, token cu bi revoke.
- Luu refresh token hash trong DB (khong luu plain text).
- Logout 1 thiet bi / all devices.
- RBAC + permission theo module/hanh dong.
- Encrypt data nhay cam (at-rest + in-transit).
- Audit log bat buoc cho thao tac benh an, toa thuoc, thanh toan.
- Rate limit login/API nhay cam bang Redis.
- Soft-delete va data retention policy.

## 5) Redis plan (bat buoc)

Muc dich su dung Redis:

- API response cache cho dashboard, lookup danh muc.
- Session/token metadata cache cho auth service.
- Refresh token jti blacklist/revoke cache.
- Distributed lock cho job tranh chay trung.
- Rate limiting (fixed window/sliding window).
- Queue lightweight (neu can) cho task don gian, uu tien RabbitMQ cho event chinh.

Checklist implementation:

1. Cau hinh Redis client + health check.
2. Define key naming convention.
3. TTL strategy theo loai du lieu.
4. Cache invalidation theo event RabbitMQ.
5. Fallback khi Redis down (graceful degradation).

## 6) RabbitMQ messaging plan (bat buoc)

Event-driven flow can co:

- AppointmentCreated -> NotificationService gui nhac lich.
- AppointmentUpdated/Cancelled -> NotificationService cap nhat nguoi benh.
- PrescriptionIssued -> NotificationService thong bao toa thuoc.
- InvoiceCreated/Paid -> thong bao thanh toan.
- MedicalRecordFinalized -> event cho Reporting/Analytics.

Ky thuat bat buoc:

- Exchange theo domain (topic/direct).
- Message contract versioning (`v1`, `v2`).
- Idempotent consumer (tranh xu ly trung).
- Retry policy + dead-letter queue.
- CorrelationId cho trace cross-service.
- Outbox pattern tai producer service de dam bao khong mat event.

## 7) API Gateway + Auth plan

- Gateway xac thuc access token.
- Internal service-to-service auth (JWT machine token/mtls giai doan sau).
- Standard response envelope va error code.
- Centralized rate limit + request size limit + CORS policy.
- Audit/security headers.

## 8) Data architecture plan

- Moi microservice so huu DB rieng.
- Khong query join truc tiep giua service.
- Cross-service read dung API call hoac read model/denormalized projection.
- Migration strategy cho tung service.
- Seed data rieng cho dev/staging.

## 9) Observability & operations

- Structured logging (JSON) + log correlation id.
- Distributed tracing (OpenTelemetry).
- Metrics (request latency, error rate, queue lag, cache hit ratio).
- Alerting: API 5xx spike, queue backlog, DB connection issue, login failure burst.
- Health checks: liveness/readiness cho service, Redis, RabbitMQ, DB.

## 10) DevOps plan

- Dockerfile cho tung service + docker-compose dev stack.
- CI pipeline:
1. Build
2. Unit tests
3. Integration tests
4. Security scan (dependency/container)
5. Publish artifact/image

- CD pipeline:
1. Deploy staging
2. Smoke test
3. Manual approval
4. Deploy production (rolling/blue-green tuy ha tang)

- Environment strategy: dev, staging, prod.
- Secret management: khong commit secret, dung vault/env secret store.

## 11) Testing strategy

- Unit tests: domain rules, application use cases.
- Integration tests: DB, Redis, RabbitMQ, external adapters.
- Contract tests: API va message contract.
- E2E tests: luong dat lich -> kham -> toa thuoc -> thanh toan -> thong bao.
- Performance tests: peak hour booking, dashboard query load.
- Chaos/failure tests (giai doan sau): Redis/RabbitMQ timeout, partial outage.

## 12) Roadmap trien khai theo phase

## Phase 0 - Alignment & foundation (1-2 tuan)

- Chot domain boundaries va glossary nghiep vu.
- Chot coding conventions, API conventions, message conventions.
- Setup observability minimum + CI baseline.
- Tao plan migration tu monolith sang microservices.

Deliverables:
- ADRs (architecture decision records), service boundaries, coding standards.

## Phase 1 - Core auth + gateway + cache + messaging skeleton (2-4 tuan)

- Identity service: login, access/refresh token, rotate/revoke.
- Redis setup: rate limit + token metadata cache.
- RabbitMQ setup: exchange/queue/dlq/retry.
- API Gateway setup: route + auth middleware.

Deliverables:
- Dang nhap on dinh, refresh token flow production-ready, messaging infra chay duoc.

## Phase 2 - Tach 3 domain uu tien cao (4-8 tuan)

- Patient service.
- Appointment service.
- Notification service (consumer RabbitMQ + sms/email push).

Deliverables:
- Luong dat lich + nhac lich end-to-end.

## Phase 3 - EMR + Prescription + Billing (6-10 tuan)

- EMR service.
- Prescription/Pharmacy service.
- Billing/Payment service.

Deliverables:
- Luong kham benh day du tu tiep nhan den thanh toan.

## Phase 4 - Reporting + optimization + compliance hardening (4-6 tuan)

- Reporting service + dashboard KPI.
- Data retention, audit enhancement, security hardening.
- Performance tuning va chaos rehearsal.

Deliverables:
- Ban van hanh gan production cho benh vien tu.

## 13) Backlog chi tiet theo nhom tinh nang

## 13.1 Identity & Access backlog

- [ ] Register/Login/Logout.
- [ ] Refresh token rotation + revoke family token khi nghi ngo leak.
- [ ] Forgot password + reset flow.
- [ ] MFA (phase sau).
- [ ] RBAC + permission matrix chi tiet.
- [ ] Login audit + suspicious activity detection.

## 13.2 Patient backlog

- [ ] CRUD benh nhan + tim kiem nang cao.
- [ ] Merge duplicate patient.
- [ ] Lich su kham tong hop.
- [ ] Quan ly nguoi than/lien he khan cap.

## 13.3 Appointment backlog

- [ ] Dat lich theo khung gio bac si.
- [ ] Rule tranh trung lich.
- [ ] Check-in, queue, trang thai kham.
- [ ] Huy/doi lich co policy.

## 13.4 EMR backlog

- [ ] Tao benh an theo mau.
- [ ] Tien trinh kham va ket luan.
- [ ] Dinh kem tai lieu.
- [ ] Signature/approval workflow.

## 13.5 Prescription backlog

- [ ] Ke toa + line item.
- [ ] Rule dosage validation.
- [ ] Canh bao trung lap/tuong tac thuoc.
- [ ] Lich su cap thuoc.

## 13.6 Billing backlog

- [ ] Catalog dich vu.
- [ ] Tao hoa don tu dich vu + toa thuoc.
- [ ] Thanh toan (tien mat/chuyen khoan/gateway).
- [ ] Hoan tien/dieu chinh hoa don.

## 13.7 Notification backlog

- [ ] Template thong bao.
- [ ] Queue consumer + retry + DLQ monitor.
- [ ] Kenh SMS/Email/Zalo.
- [ ] Log trang thai gui/that bai.

## 13.8 Reporting backlog

- [ ] Dashboard theo ngay/tuan/thang.
- [ ] KPI van hanh (cho kham, no-show, tai kham).
- [ ] KPI tai chinh.

## 14) Definition of Done (DoD)

Moi tinh nang chi duoc xem la hoan thanh khi:

1. Dung layer architecture (Domain -> Application -> Infrastructure -> API).
2. Co unit test + integration test lien quan.
3. Co log + metric co y nghia.
4. Co API doc/message contract doc.
5. Qua security checklist (authz, input validation, secret, audit).
6. Qua smoke test tren staging.

## 15) Risks & mitigation

- Risk: Tach microservice qua som gay tang do phuc tap.
- Mitigation: Lam ro boundaries trong modular monolith truoc.

- Risk: Message duplicate/lost.
- Mitigation: Outbox + idempotency + DLQ + monitoring.

- Risk: Token leak.
- Mitigation: refresh token rotation, revoke, device tracking, short access token TTL.

- Risk: Performance bottleneck dashboard.
- Mitigation: Redis cache + read model + async projection.

## 16) Thu tu thuc hien de bam theo ngay

1. Chot boundaries + ADR + coding conventions.
2. Lam Auth + Refresh token + Redis + Gateway.
3. Dung RabbitMQ infrastructure + Notification consumer.
4. Tach Patient va Appointment service.
5. Hoan thien luong kham (EMR -> Prescription -> Billing).
6. Bo sung reporting, hardening, optimization.

