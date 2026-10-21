// 网页版《港硕生活模拟器 · 轻松版四轮样例 a320095》的规则引擎（从 开始游戏.html 的构建产物中原样截取，未改动规则），
// 只用于和 Unity 版 C# 规则逐项比对：node engine.js cases.json js.json
var Kt = Date.UTC(2026, 7, 31),
  qt = { 1: 0, 2: 133 };
function Jt(e) {
  return new Date(Kt + e * 864e5);
}
function Yt(e) {
  return Jt(e).toISOString().slice(0, 10);
}
function Xt(e) {
  return ((e % 7) + 7) % 7;
}
function Zt(e) {
  let [t, n, r] = e.split(`-`).map(Number);
  return Math.round((Date.UTC(t, n - 1, r) - Kt) / 864e5);
}
function Qt() {
  let e = [];
  for (let t = 0; t < 8; t++) {
    let n = qt[1] + t * 14;
    e.push({
      index: t,
      kind: `semester`,
      semester: 1,
      roundInSemester: t + 1,
      startDay: n,
      endDay: n + 14 - 1,
    });
  }
  e.push({
    index: 8,
    kind: `winter`,
    semester: null,
    roundInSemester: null,
    startDay: 112,
    endDay: 132,
  });
  for (let t = 0; t < 8; t++) {
    let n = qt[2] + t * 14;
    e.push({
      index: 9 + t,
      kind: `semester`,
      semester: 2,
      roundInSemester: t + 1,
      startDay: n,
      endDay: n + 14 - 1,
    });
  }
  return e;
}
var M = Qt();
function $t(e) {
  return M.find((t) => e >= t.startDay && e <= t.endDay) ?? null;
}
var en = new Map(
  [
    { date: `2026-09-26`, name: `中秋节翌日` },
    { date: `2026-10-01`, name: `国庆日` },
    { date: `2026-10-19`, name: `重阳节翌日（重阳节逢周日）` },
    { date: `2026-12-25`, name: `圣诞节` },
    { date: `2026-12-26`, name: `圣诞节后第一个周日` },
    { date: `2027-01-01`, name: `一月一日` },
    { date: `2027-02-06`, name: `农历年初一` },
    { date: `2027-02-08`, name: `农历年初三` },
    { date: `2027-02-09`, name: `农历年初四（年初二逢周日）` },
    { date: `2027-03-26`, name: `耶稣受难节` },
    { date: `2027-03-27`, name: `耶稣受难节翌日` },
    { date: `2027-03-29`, name: `复活节星期一` },
    { date: `2027-04-05`, name: `清明节` },
    { date: `2027-05-01`, name: `劳动节` },
  ].map((e) => [Zt(e.date), e.name]),
);
function tn(e) {
  return en.get(e) ?? null;
}
var nn = {
    1: [
      {
        course: `research`,
        code: `S1-E`,
        type: `exam`,
        weekday: 0,
        half: `am`,
      },
      {
        course: `culture`,
        code: `S1-C`,
        type: `comprehensive`,
        weekday: 1,
        half: `pm`,
      },
      {
        course: `media`,
        code: `S1-P1`,
        type: `project`,
        weekday: 2,
        half: `am`,
      },
      {
        course: `design`,
        code: `S1-P2`,
        type: `project`,
        weekday: 3,
        half: `pm`,
      },
    ],
    2: [
      { course: `s2-exam`, code: `S2-E`, type: `exam`, weekday: 0, half: `am` },
      {
        course: `s2-comprehensive`,
        code: `S2-C`,
        type: `comprehensive`,
        weekday: 1,
        half: `pm`,
      },
      {
        course: `s2-project-1`,
        code: `S2-P1`,
        type: `project`,
        weekday: 2,
        half: `am`,
      },
      {
        course: `s2-project-2`,
        code: `S2-P2`,
        type: `project`,
        weekday: 3,
        half: `pm`,
      },
    ],
  },
  rn = [
    { type: `exam`, kind: `midterm`, week: 8 },
    { type: `exam`, kind: `final`, week: 16 },
    { type: `comprehensive`, kind: `final`, week: 16 },
  ],
  an = (e, t) => `${e}:${t}`;
function on(e) {
  let t = [];
  for (let n = 1; n <= 16; n++)
    for (let r of nn[e]) {
      let i = qt[e] + (n - 1) * 7 + r.weekday,
        a = rn.find((e) => e.type === r.type && e.week === n)?.kind ?? `class`;
      t.push({
        id: `${r.code}-w${String(n).padStart(2, `0`)}${a === `class` ? `` : `-${a}`}`,
        semester: e,
        week: n,
        course: r.course,
        code: r.code,
        type: r.type,
        kind: a,
        day: i,
        half: r.half,
        plannedDay: i,
        plannedHalf: r.half,
        moveReason: null,
      });
    }
  return t;
}
function sn(e) {
  let t = on(e),
    n = qt[e] + 112 - 1,
    r = t.filter((e) => tn(e.day) !== null),
    i = new Set(t.filter((e) => !r.includes(e)).map((e) => an(e.day, e.half))),
    a = new Set(
      t.filter((e) => !r.includes(e)).map((e) => `${e.course}:${e.day}`),
    );
  for (let e of r) {
    let t = tn(e.plannedDay),
      r = e.plannedHalf === `am` ? `pm` : `am`,
      o = !1;
    for (let s = e.plannedDay + 1; s <= n && !o; s++)
      if (!(Xt(s) >= 5 || tn(s) !== null || a.has(`${e.course}:${s}`))) {
        for (let n of [e.plannedHalf, r])
          if (!i.has(an(s, n))) {
            (e.day = s),
              (e.half = n),
              (e.moveReason = `${Yt(e.plannedDay)} 为公众假期（${t}），顺延至本学期最早的空闲半日`),
              i.add(an(s, n)),
              a.add(`${e.course}:${s}`),
              (o = !0);
            break;
          }
      }
    if (!o) throw Error(`无法为 ${e.id} 找到补课时段`);
  }
  return t.sort(
    (e, t) =>
      e.day - t.day || (e.half === t.half ? 0 : e.half === `am` ? -1 : 1),
  );
}
var cn = [...sn(1), ...sn(2)];
cn.filter((e) => e.moveReason !== null);
function ln(e, t) {
  return cn.find((n) => n.day === e && n.half === t) ?? null;
}
var un = {
    "exam:midterm": { weekday: 3, half: `am` },
    "exam:final": { weekday: 4, half: `am` },
    "comprehensive:final": { weekday: 4, half: `pm` },
  },
  dn = cn.filter((e) => e.kind !== `class`);
function fn(e) {
  let t = dn.find((t) => t.id === e);
  if (!t) return null;
  let n = un[`${t.type}:${t.kind}`];
  return { day: qt[t.semester] + 105 + n.weekday, half: n.half };
}
function pn(e, t) {
  return (
    dn.find((n) => {
      let r = fn(n.id);
      return r.day === e && r.half === t;
    }) ?? null
  );
}
for (let e of dn) {
  let t = fn(e.id),
    n = qt[e.semester] + 112 - 1;
  if (t.day <= e.day || t.day > n || ln(t.day, t.half) || tn(t.day))
    throw Error(`补考时段无效：${e.id}`);
}
var mn = [
    {
      id: `fast_food`,
      name: `快餐店店员`,
      requirements: {},
      pay: 220,
      fatigue: 10,
      stress: 3,
      baseChance: 85,
      growth: `practice`,
      growthPerShift: 0.2,
    },
    {
      id: `retail`,
      name: `零售店员`,
      requirements: { communication: 25 },
      pay: 260,
      fatigue: 8,
      stress: 4,
      baseChance: 70,
      growth: `communication`,
      growthPerShift: 0.2,
    },
    {
      id: `event_assistant`,
      name: `活动助理`,
      requirements: { communication: 40 },
      pay: 320,
      fatigue: 9,
      stress: 6,
      baseChance: 65,
      growth: `communication`,
      growthPerShift: 0.3,
    },
    {
      id: `admin`,
      name: `行政文员`,
      requirements: { writing: 40, practice: 30 },
      pay: 340,
      fatigue: 6,
      stress: 5,
      baseChance: 65,
      growth: `writing`,
      growthPerShift: 0.3,
    },
    {
      id: `tutor`,
      name: `补习导师`,
      requirements: { research: 55, communication: 50 },
      pay: 360,
      fatigue: 6,
      stress: 7,
      baseChance: 60,
      growth: `communication`,
      growthPerShift: 0.4,
    },
    {
      id: `social_media`,
      name: `社交媒体运营`,
      requirements: { writing: 55, practice: 50 },
      pay: 400,
      fatigue: 6,
      stress: 6,
      baseChance: 55,
      growth: `writing`,
      growthPerShift: 0.4,
    },
    {
      id: `research_assistant`,
      name: `大学兼职研究助理`,
      requirements: { research: 70, writing: 55 },
      pay: 500,
      fatigue: 5,
      stress: 8,
      baseChance: 50,
      growth: `research`,
      growthPerShift: 0.4,
    },
  ],
  hn = { surplusFactor: 0.5, min: 30, max: 90 };
function gn(e) {
  return mn.find((t) => t.id === e) ?? null;
}
function _n(e, t) {
  return Object.entries(e.requirements)
    .filter(([e, n]) => t[e] < n)
    .map(([e, n]) => ({ ability: e, required: n, current: t[e] }));
}
function vn(e, t) {
  return _n(e, t).length === 0;
}
function yn(e, t) {
  if (!vn(e, t)) return null;
  let n = Object.entries(e.requirements);
  if (n.length === 0) return e.baseChance;
  let r = n.reduce((e, [n, r]) => e + (t[n] - r), 0) / n.length,
    i = Math.min(hn.max, Math.max(hn.min, e.baseChance + hn.surplusFactor * r));
  return Math.round(i * 10) / 10;
}
var bn = () => ({ hired: [], lastAttempt: null });
function xn(e, t) {
  return e.lastAttempt?.round === t;
}
function Sn(e, t, n, r, i) {
  let a = gn(n);
  if (!a) return { market: e, error: `未知岗位。`, attempt: null };
  if (xn(e, t))
    return {
      market: e,
      error: `本轮的求职机会已经用过了，下一轮再试。`,
      attempt: null,
    };
  if (e.hired.includes(n))
    return {
      market: e,
      error: `你已经获得这个岗位，不需要再申请。`,
      attempt: null,
    };
  let o = yn(a, r);
  if (o === null)
    return { market: e, error: `尚未达到申请门槛。`, attempt: null };
  if (!Number.isFinite(i) || i < 0 || i >= 1)
    throw RangeError(`随机数必须在 [0,1)：${i}`);
  let s = { round: t, job: n, chance: o, roll: i, success: i < o / 100 };
  return {
    market: {
      hired: s.success ? [...e.hired, n] : [...e.hired],
      lastAttempt: s,
    },
    error: null,
    attempt: s,
  };
}
var N = [
    {
      id: `research`,
      semester: 1,
      name: `研究方法`,
      teacher: `陈老师`,
      assignment: `把问题问清楚`,
      requirement: `选择一个香港日常生活现象，写出研究问题、访谈提纲与研究伦理说明。`,
      project: `题目待定`,
    },
    {
      id: `culture`,
      semester: 1,
      name: `城市与文化`,
      teacher: `林老师`,
      assignment: `街区观察札记`,
      requirement: `观察一处街区，将课堂的空间与身份概念用于一篇图文分析。`,
      project: `题目待定`,
    },
    {
      id: `media`,
      semester: 1,
      name: `数字传播`,
      teacher: `黄老师`,
      assignment: `平台案例分析`,
      requirement: `选择一个本地传播案例，分析受众、内容策略与平台机制。`,
      project: `一份给新来港同学的城市指南`,
    },
    {
      id: `design`,
      semester: 1,
      name: `设计思维`,
      teacher: `周老师`,
      assignment: `从痛点到原型`,
      requirement: `记录一项学生生活需求，提出方案，并用简图呈现测试方法。`,
      project: `题目待定`,
    },
    {
      id: `s2-exam`,
      semester: 2,
      name: `第二学期考试主导课`,
      teacher: `任课老师`,
      assignment: `个人作业（题目待定）`,
      requirement: `课程名称与作业要求待定。`,
      project: `题目待定`,
    },
    {
      id: `s2-comprehensive`,
      semester: 2,
      name: `第二学期综合课`,
      teacher: `任课老师`,
      assignment: `个人作业（题目待定）`,
      requirement: `课程名称与作业要求待定。`,
      project: `题目待定`,
    },
    {
      id: `s2-project-1`,
      semester: 2,
      name: `第二学期项目课一`,
      teacher: `任课老师`,
      assignment: `个人作业（题目待定）`,
      requirement: `课程名称与作业要求待定。`,
      project: `题目待定`,
    },
    {
      id: `s2-project-2`,
      semester: 2,
      name: `第二学期项目课二`,
      teacher: `任课老师`,
      assignment: `个人作业（题目待定）`,
      requirement: `课程名称与作业要求待定。`,
      project: `题目待定`,
    },
  ],
  Cn = {
    E: {
      individual: { releaseWeek: 3, dueWeek: 6, workloadUnits: 2 },
      group: { releaseWeek: 4, dueWeek: 10, workloadUnits: 2 },
    },
    C: {
      individual: { releaseWeek: 3, dueWeek: 7, workloadUnits: 3 },
      group: { releaseWeek: 4, dueWeek: 12, workloadUnits: 3 },
    },
    P1: {
      individual: { releaseWeek: 3, dueWeek: 9, workloadUnits: 4 },
      group: { releaseWeek: 4, dueWeek: 13, workloadUnits: 6 },
    },
    P2: {
      individual: { releaseWeek: 3, dueWeek: 10, workloadUnits: 4 },
      group: { releaseWeek: 4, dueWeek: 14, workloadUnits: 6 },
    },
  };
function wn(e) {
  return nn[N.find((t) => t.id === e).semester].find((t) => t.course === e);
}
function Tn(e) {
  return Cn[wn(e).code.split(`-`)[1]];
}
function En(e, t) {
  return qt[N.find((t) => t.id === e).semester] + (t - 1) * 7 + 6;
}
var Dn = [
    {
      id: `disney`,
      name: `迪士尼乐园`,
      text: `暂时把 DDL 留在手机里，看一次巡游与灯光。`,
      cg: `trip_disney`,
    },
    {
      id: `peak`,
      name: `太平山顶`,
      text: `从高处看城市与海港，天色慢慢变化。`,
      cg: `trip_peak`,
    },
    {
      id: `maclehose`,
      name: `麦理浩径`,
      text: `离开密集楼宇，沿山路走进更开阔的风景。`,
      cg: `trip_maclehose`,
    },
    {
      id: `wong_tai_sin`,
      name: `黄大仙庙`,
      text: `在人流与庭院之间放慢步子，为接下来的学期许个愿。`,
      cg: `trip_wong_tai_sin`,
    },
    {
      id: `palace_museum`,
      name: `香港故宫博物馆`,
      text: `看展、看海，挑一件想记住的展品。`,
      cg: `trip_palace_museum`,
    },
    {
      id: `victoria_harbour`,
      name: `维多利亚港`,
      text: `看渡轮经过，等天色从蓝色变成夜景。`,
      cg: `trip_victoria_harbour`,
    },
  ],
  On = { cost: 300, fatigue: 20, stress: 30 },
  kn = (e) => `trip:${e}`,
  P = (e) => (e?.startsWith(`trip:`) ? e.slice(5) : null),
  An = (e) => Dn.find((t) => t.id === e).name,
  jn = (e) => `makeup:${e}`,
  Mn = (e) => (e?.startsWith(`makeup:`) ? e.slice(7) : null),
  Nn = [
    {
      id: `review`,
      name: `预习与复习`,
      mark: `读`,
      description: `整理课堂笔记；不推进尚未布置的作业。`,
      fatigue: 4,
      stress: 1,
      money: 0,
      cap: 28,
    },
    {
      id: `rest`,
      name: `在家休息`,
      mark: `休`,
      description: `让这半天慢一点；恢复量取决于房型与已摆放的家具。`,
      fatigue: 0,
      stress: 0,
      money: 0,
      cap: 28,
    },
    {
      id: `walk`,
      name: `外出散步`,
      mark: `逛`,
      description: `去公园或海旁走走。花费 HK$45，精力 +4、压力 −8；可能遇到小事件。`,
      fatigue: -4,
      stress: -8,
      money: -45,
      cap: 28,
    },
  ],
  Pn = { fatigue: 3, stress: 2 },
  Fn = {
    base: 10,
    mastery: 0.7,
    research: 0.2,
    penaltyFrom: 60,
    penaltyRate: 0.15,
  },
  In = [
    { id: `hk`, name: `留港就业` },
    { id: `mainland`, name: `回内地就业` },
    { id: `phd`, name: `继续读博` },
  ],
  Ln = {
    scholarshipCgpa100: 350,
    scholarshipAward: 8e3,
    employAbilityAvg: 55,
    employJobShifts: 24,
    phdCgpa100: 330,
    phdResearch: 70,
    phdWriting: 60,
    phdCourseMin: 80,
  },
  Rn = {
    individual: { name: `个人作业`, mark: `写`, fatigue: 6, stress: 3 },
    group: { name: `小组作业`, mark: `组`, fatigue: 5, stress: 3 },
  },
  zn = {
    pointsPerUnit: 20,
    progress: { scale: 10, base: 0.75, divisor: 120 },
    quality: { base: 50, ability: 0.25, mastery: 0.15, max: 90 },
    polish: [4, 4, 3, 3, 2, 2],
    polishAfter: 1,
    cap: 100,
  },
  Bn = [
    { id: `research`, name: `资料研究` },
    { id: `writing`, name: `报告撰写` },
    { id: `presentation`, name: `汇报展示／原型执行` },
  ],
  Vn = [
    {
      id: `tea`,
      name: `港式凉茶`,
      kind: `item`,
      price: 18,
      description: `使用后压力 −8`,
      stock: 2,
    },
    {
      id: `bun`,
      name: `菠萝包`,
      kind: `item`,
      price: 12,
      description: `使用后精力 +6`,
      stock: 2,
    },
    {
      id: `lamp`,
      name: `二手阅读灯`,
      kind: `furniture`,
      price: 120,
      description: `摆放后，每个个人作业半日少消耗 2 精力`,
      stock: 1,
    },
    {
      id: `plant`,
      name: `窗边绿植`,
      kind: `furniture`,
      price: 60,
      description: `摆放后，每个休息半日额外恢复 2 压力`,
      stock: 1,
    },
    {
      id: `cushion`,
      name: `柔软靠垫`,
      kind: `furniture`,
      price: 85,
      description: `摆放后，每个休息半日额外恢复 3 精力`,
      stock: 1,
    },
  ],
  Hn = [
    {
      id: `frugal`,
      name: `拮据`,
      budget: 1e5,
      recommended: `compact`,
      description: `基本覆盖便宜住房和必要开销；购物、旅游及改善生活通常需要兼职额外赚钱。`,
    },
    {
      id: `balanced`,
      name: `中等均衡`,
      budget: 125e3,
      recommended: `standard`,
      description: `足以覆盖正常房租和生活开销，可偶尔额外消费；频繁旅游、购物或升级住房时需兼职补贴。`,
    },
    {
      id: `wealthy`,
      name: `富裕`,
      budget: 2e5,
      recommended: `comfort`,
      description: `生活费较充裕，可负担日常开支，并有更多购物、旅游空间；兼职压力较小。`,
    },
  ],
  Un = [
    {
      id: `compact`,
      name: `紧凑单间`,
      area: `4㎡`,
      monthlyRent: 6e3,
      slots: 4,
      restFatigue: 12,
      restStress: 8,
    },
    {
      id: `standard`,
      name: `标准房间`,
      area: `10㎡`,
      monthlyRent: 7500,
      slots: 7,
      restFatigue: 18,
      restStress: 12,
    },
    {
      id: `comfort`,
      name: `舒适房间`,
      area: null,
      monthlyRent: 12e3,
      slots: 10,
      restFatigue: 24,
      restStress: 16,
    },
  ],
  Wn = { fatigue: 6, stress: 4 };
function Gn(e) {
  return Un.find((t) => t.id === e);
}
function Kn(e) {
  let t = Gn(e);
  return { annualRent: t.monthlyRent * 12, deposit: t.monthlyRent };
}
var qn = [
  { id: `research`, name: `研究分析` },
  { id: `writing`, name: `写作表达` },
  { id: `communication`, name: `沟通展示` },
  { id: `practice`, name: `实践执行` },
];
function Jn(e) {
  return qn.find((t) => t.id === e).name;
}
var Yn = {
    research: `research`,
    culture: `writing`,
    media: `communication`,
    design: `practice`,
    "s2-exam": `research`,
    "s2-comprehensive": `writing`,
    "s2-project-1": `communication`,
    "s2-project-2": `practice`,
  },
  Xn = {
    class: { ability: 0.5, mastery: 5 },
    review: { ability: 0.5, mastery: 8 },
    study: { ability: 0.5, mastery: 2 },
    group: { ability: 0.5, mastery: 1 },
  },
  F = {
    startFatigue: 18,
    startStress: 15,
    dailyCost: 75,
    dailyRecovery: 3,
    dailyCalm: 1,
    medical: 180,
    illness: 90,
    illnessFatigueRelief: 32,
    illnessStressRelief: 20,
  },
  Zn = [`周一`, `周二`, `周三`, `周四`, `周五`, `周六`, `周日`],
  Qn = { am: `上午`, pm: `下午` };
function I(e) {
  let t = Jt(e);
  return `${t.getUTCMonth() + 1}月${t.getUTCDate()}日`;
}
function $n(e) {
  return N.find((t) => t.id === e).name;
}
function er(e) {
  return Nn.find((t) => t.id === e);
}
var tr = (e) => `task:${e}`,
  nr = (e) => `project:${e}`;
function rr(e) {
  return e?.startsWith(`task:`)
    ? { kind: `individual`, course: e.slice(5) }
    : e?.startsWith(`project:`)
      ? { kind: `group`, course: e.slice(8) }
      : null;
}
function ir(e) {
  let t = Mn(e);
  if (t) {
    let e = dn.find((e) => e.id === t);
    return e
      ? `补考 · ${$n(e.course)}${e.kind === `midterm` ? `期中` : `期末`}`
      : `补考`;
  }
  let n = P(e);
  if (n) return `全天游玩 · ${An(n)}`;
  let r = rr(e);
  return r
    ? `${Rn[r.kind].name} · ${$n(r.course)}`
    : e?.startsWith(`class:`)
      ? $n(e.slice(6))
      : e?.startsWith(`exam:`)
        ? `考试 · ${$n(e.slice(5))}`
        : e?.startsWith(`job:`)
          ? `兼职 · ${mn.find((t) => t.id === e.slice(4))?.name ?? `岗位`}`
          : (er(e)?.name ?? `留白`);
}
var ar = { min: 20, max: 70, maxSum: 210, needOneAtLeast: 40 },
  or = { start: 50, min: 0, max: 100 },
  sr = {
    on_time: { affection: 5, reputation: 2 },
    late: { affection: -3, reputation: -2 },
    missing: { affection: -6, reputation: -12 },
  },
  cr = {
    A: 4,
    "A-": 3,
    "B+": 2,
    B: 1,
    "B-": 0,
    "C+": -1,
    C: -2,
    "C-": -3,
    F: -4,
  },
  lr = {
    base: 70,
    pivot: 40,
    abilityFactor: 0.8,
    affectionFactor: 0.4,
    reputationFactor: 0.2,
    min: 15,
    max: 95,
  },
  ur = (e) => Math.max(or.min, Math.min(or.max, e));
function dr(e) {
  return qn.reduce((t, n) => t + e[n.id], 0) / qn.length;
}
function fr(e, t, n) {
  let r = lr,
    i =
      r.base -
      r.abilityFactor * (e - r.pivot) +
      r.affectionFactor * (t - or.start) +
      r.reputationFactor * (n - or.start);
  return Math.round(Math.max(r.min, Math.min(r.max, i)) * 10) / 10;
}
function pr(e, t) {
  return t === `research`
    ? `research`
    : t === `writing`
      ? `writing`
      : wn(e).type === `project`
        ? `practice`
        : `communication`;
}
function mr(e, t) {
  return t === `research`
    ? `资料研究`
    : t === `writing`
      ? `报告撰写`
      : wn(e).type === `project`
        ? `原型执行`
        : `汇报展示`;
}
function hr(e, t) {
  return sr[e].reputation + (e === `missing` || t === null ? 0 : cr[t]);
}
function gr(e, t, n) {
  return ur(e + hr(t, n));
}
function _r(e) {
  let t = e >>> 0;
  return () => {
    t = (t + 1831565813) >>> 0;
    let e = t;
    return (
      (e = Math.imul(e ^ (e >>> 15), e | 1)),
      (e ^= e + Math.imul(e ^ (e >>> 7), e | 61)),
      ((e ^ (e >>> 14)) >>> 0) / 4294967296
    );
  };
}
function vr(...e) {
  let t = 2166136261;
  for (let n of e) t = Math.imul(t ^ (n | 0), 16777619) >>> 0;
  return _r(t)();
}
function yr(e, t) {
  let n = _r(t),
    r = [...e];
  for (let e = r.length - 1; e > 0; e--) {
    let t = Math.floor(n() * (e + 1));
    [r[e], r[t]] = [r[t], r[e]];
  }
  return r;
}
var br = [
    `陈`,
    `林`,
    `黄`,
    `张`,
    `李`,
    `王`,
    `吴`,
    `刘`,
    `蔡`,
    `杨`,
    `郑`,
    `梁`,
    `何`,
    `许`,
    `周`,
    `叶`,
    `罗`,
    `冯`,
    `邓`,
    `谢`,
  ],
  xr =
    `嘉怡.子轩.晓彤.俊熙.思颖.浩然.咏琪.家乐.美琳.志豪.雅婷.卓谦.凯琳.永康.芷晴.锦华.颖欣.伟诚.可欣.柏言.诗韵.国栋.佩珊.天佑.慧敏.启明.若彤.展鹏`.split(
      `.`,
    );
function Sr(e) {
  let t = _r(e),
    n = new Set(),
    r = () => {
      for (;;) {
        let e =
          br[Math.floor(t() * br.length)] + xr[Math.floor(t() * xr.length)];
        if (!n.has(e)) return n.add(e), e;
      }
    },
    i = ar,
    a = [];
  for (let e = 0; e < 24; e++) {
    let n;
    do n = qn.map(() => i.min + Math.floor(t() * (i.max - i.min + 1)));
    while (
      n.reduce((e, t) => e + t, 0) > i.maxSum ||
      Math.max(...n) < i.needOneAtLeast
    );
    a.push({
      id: `c${String(e + 1).padStart(2, `0`)}`,
      name: r(),
      abilities: Object.fromEntries(qn.map((e, t) => [e.id, n[t]])),
      kind: `pool`,
    });
  }
  for (let e = 0; e < 2; e++)
    a.push({
      id: `f${e + 1}`,
      name: r(),
      abilities: Object.fromEntries(qn.map((e) => [e.id, 30])),
      kind: `fallback`,
    });
  return a;
}
var Cr = {
    project: { individual: 40, group: 60 },
    comprehensive: { individual: 30, group: 30, final: 40 },
    exam: { individual: 20, group: 20, midterm: 20, final: 40 },
  },
  wr = [
    { min: 85, letter: `A`, points100: 400 },
    { min: 80, letter: `A-`, points100: 367 },
    { min: 75, letter: `B+`, points100: 333 },
    { min: 70, letter: `B`, points100: 300 },
    { min: 65, letter: `B-`, points100: 267 },
    { min: 60, letter: `C+`, points100: 233 },
    { min: 55, letter: `C`, points100: 200 },
    { min: 50, letter: `C-`, points100: 167 },
    { min: 0, letter: `F`, points100: 0 },
  ],
  Tr = 1e-9;
function Er(e) {
  return e.replace(`-`, `−`);
}
function Dr(e) {
  return Object.keys(Cr[e]);
}
function Or(e, t) {
  if (!Number.isFinite(t) || t < 0 || t > 100)
    throw RangeError(`${e} 分数必须在 0—100：${t}`);
}
function kr(e, t) {
  let n = Cr[e];
  for (let [r, i] of Object.entries(t))
    if (i !== void 0) {
      if (!(r in n)) throw RangeError(`${e} 类课程没有 ${r} 考核`);
      Or(r, i);
    }
  let r = 0;
  for (let i of Dr(e)) {
    let e = t[i];
    if (e === void 0) return null;
    r += e * n[i];
  }
  return r / 100;
}
function Ar(e) {
  return Or(`课程总分`, e), wr.find((t) => e + Tr >= t.min);
}
function jr(e, t, n = !1) {
  let r = kr(e, t);
  if (n)
    return {
      status: `graded`,
      total: r,
      letter: `F`,
      points100: 0,
      gradePoints: 0,
      passed: !1,
      forcedFail: !0,
    };
  if (r === null) return { status: `in_progress` };
  let i = Ar(r);
  return {
    status: `graded`,
    total: r,
    letter: i.letter,
    points100: i.points100,
    gradePoints: i.points100 / 100,
    passed: i.letter !== `F`,
    forcedFail: !1,
  };
}
function Mr(e) {
  let t = e.filter((e) => e.status === `graded`);
  return { sum100: t.reduce((e, t) => e + t.points100, 0), count: t.length };
}
function Nr(e) {
  let { sum100: t, count: n } = Mr(e);
  return n === 0 ? null : t / n / 100;
}
function Pr(e) {
  return e === null ? `—` : e.toFixed(2);
}
function Fr(e) {
  let t = Nr(e);
  if (e.length < 8 || e.some((e) => e.status !== `graded`))
    return { status: `pending`, cgpa: t };
  let { sum100: n, count: r } = Mr(e),
    i = e.flatMap((e, t) => (e.status === `graded` && !e.passed ? [t] : [])),
    a = n < 200 * r;
  return {
    status: i.length === 0 && !a ? `graduated` : `delayed`,
    cgpa: t,
    failedCourses: i,
    cgpaBelowThreshold: a,
  };
}
function Ir(e, t = null) {
  if (e === `missing`) return 0;
  if (t === null) throw RangeError(`按时或迟交提交的作业需要原始分`);
  return Or(`作业原始分`, t), e === `late` ? (t * 80) / 100 : t;
}
var Lr = {
    perRoundCap: 2,
    cooldownRounds: 2,
    walkChance: 0.25,
    groupChance: 0.25,
    dailyChance: 0.05,
    categoryCap: { group: 1, daily: 1 },
    firstWalkEvent: `red_bulbul`,
  },
  Rr = [
    {
      id: `red_bulbul`,
      trigger: `walk`,
      title: `在公园看到了红耳鹎！`,
      text: `公园的绿荫里，一只红耳鹎停在枝头，歪着头看你。`,
      cg: `event_red_bulbul`,
      source: `用户原创（D72）：选项文案与数值保留原文，效果叠加在散步基础恢复上（D80）`,
      enabled: !0,
      options: [
        {
          label: `呼唤：嘬嘬嘬~`,
          result: `小鸟给了你一个鄙视的眼神后飞走了，不是万物皆可嘬嘬。`,
          fatigue: -3,
          stress: -5,
          money: 0,
        },
        {
          label: `拍照：关闭闪光灯，记录美好时刻。`,
          result: `相册里多了一只杀马特造型腮红鸡！每次看到它心情都会好起来。`,
          fatigue: -4,
          stress: -8,
          money: 0,
        },
      ],
    },
    {
      id: `walk_shower`,
      trigger: `walk`,
      title: `突然下起短暂阵雨`,
      text: `刚走到街角，雨点就落了下来。`,
      cg: `event_walk_shower`,
      source: `Astra 初稿，用户批准启用（D85）`,
      enabled: !0,
      options: [
        {
          label: `到檐下听雨`,
          result: `雨声把脑子里的待办清单冲淡了一点。`,
          fatigue: -2,
          stress: -3,
          money: 0,
        },
        {
          label: `买一杯热饮等雨停`,
          result: `热饮下肚，雨也停了。`,
          fatigue: -3,
          stress: -5,
          money: -20,
        },
      ],
    },
    {
      id: `walk_music`,
      trigger: `walk`,
      title: `广场边传来熟悉的旋律`,
      text: `有人在广场边弹唱一首你很熟悉的歌。`,
      cg: `event_walk_music`,
      source: `Astra 初稿，用户批准启用（D85）`,
      enabled: !0,
      options: [
        {
          label: `坐下听完`,
          result: `一曲终了，你跟着周围的人一起鼓掌。`,
          fatigue: 0,
          stress: -5,
          money: 0,
        },
        {
          label: `跟着轻声哼唱`,
          result: `你小声跟着哼完了副歌。`,
          fatigue: 0,
          stress: -3,
          money: 0,
        },
      ],
    },
    {
      id: `walk_sky`,
      trigger: `walk`,
      title: `海边的云被落日染亮`,
      text: `回头时，海面上的云已经变成了橘粉色。`,
      cg: `event_walk_sky`,
      source: `Astra 初稿，用户批准启用（D85）`,
      enabled: !0,
      options: [
        {
          label: `拍一张发给家人`,
          result: `家人很快回了一个大拇指。`,
          fatigue: 0,
          stress: -5,
          money: 0,
        },
        {
          label: `把手机收起来多看一会`,
          result: `天色一点点暗下去，你多站了一会儿。`,
          fatigue: -3,
          stress: -4,
          money: 0,
        },
      ],
    },
  ],
  zr = `Astra 初稿，用户批准启用（D85）`;
Rr.push(
  {
    id: `group_outline`,
    trigger: `group`,
    stage: `production`,
    title: `组员认为切入点应该重做`,
    text: `讨论时，{npc}认为报告的切入点不够清楚，想重新收窄问题。`,
    cg: `event_group_outline`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `认真听完，再一起收窄问题`,
        result: `花了些心力，但{npc}觉得被认真对待了。`,
        fatigue: 0,
        stress: 2,
        money: 0,
        affection: 5,
      },
      {
        label: `先沿用现有框架，把建议记进讨论`,
        result: `进度没停下来，只是{npc}有点失落。`,
        fatigue: 0,
        stress: 0,
        money: 0,
        affection: -2,
      },
    ],
  },
  {
    id: `group_delay`,
    trigger: `group`,
    stage: `production`,
    title: `组员临时忙不过来`,
    text: `{npc}这周事情太多，问你能不能先接一小段。`,
    cg: `event_group_delay`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `接过来，并说明分工不变`,
        result: `多忙了一会儿，{npc}很感激。`,
        fatigue: 3,
        stress: 0,
        money: 0,
        affection: 5,
      },
      {
        label: `说明自己也满负荷，维持分工`,
        result: `你守住了自己的节奏，{npc}有些无奈。`,
        fatigue: 0,
        stress: 2,
        money: 0,
        affection: -2,
      },
    ],
  },
  {
    id: `group_credit`,
    trigger: `group`,
    stage: `finished`,
    title: `汇报页漏了组员的贡献`,
    text: `整理最后的汇报页时，你发现{npc}负责的部分没有署名。`,
    cg: `event_group_credit`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `主动补上，并向大家说明`,
        result: `{npc}看到后向你道谢，大家也觉得你做事周到。`,
        fatigue: 0,
        stress: 0,
        money: 0,
        affection: 6,
        reputation: 1,
      },
      {
        label: `提醒负责汇总的人补上`,
        result: `署名补上了，{npc}也知道是你提醒的。`,
        fatigue: 0,
        stress: 0,
        money: 0,
        affection: 2,
      },
    ],
  },
  {
    id: `group_rehearsal`,
    trigger: `group`,
    stage: `finished`,
    title: `队友提议多练一次问答`,
    text: `{npc}有点担心课堂提问，想再练一次。`,
    cg: `event_group_rehearsal`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `留下来快速演练`,
        result: `练完心里踏实多了，{npc}也松了口气。`,
        fatigue: 2,
        stress: -3,
        money: 0,
        affection: 3,
      },
      {
        label: `按计划结束，互发提示卡`,
        result: `你们各自带着提示卡回去准备。`,
        fatigue: 0,
        stress: -1,
        money: 0,
        affection: 1,
      },
    ],
  },
  {
    id: `daily_call`,
    trigger: `daily`,
    title: `家人发来消息`,
    text: `睡前，手机亮了一下：“今天过得怎么样？”`,
    cg: `event_daily_call`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `认真聊一会`,
        result: `聊完以后，这一天好像没那么累了。`,
        fatigue: 0,
        stress: -4,
        money: 0,
      },
      {
        label: `回一张今天的照片`,
        result: `家人回了一个笑脸。`,
        fatigue: 0,
        stress: -2,
        money: 0,
      },
    ],
  },
  {
    id: `daily_small_win`,
    trigger: `daily`,
    studied: !0,
    title: `看懂了以前没懂的一段`,
    text: `翻旧笔记时，你突然看懂了之前卡住的一段。`,
    cg: `event_daily_small_win`,
    source: zr,
    enabled: !0,
    options: [
      {
        label: `把发现写成一句提醒`,
        result: `你在笔记边上写下了这句提醒。`,
        fatigue: 0,
        stress: -3,
        money: 0,
      },
      {
        label: `给自己泡杯茶，庆祝一下`,
        result: `小小的进步也值得庆祝。`,
        fatigue: -2,
        stress: -2,
        money: 0,
      },
    ],
  },
);
function Br(e) {
  return Rr.find((t) => t.id === e) ?? null;
}
var Vr = [`female_young`, `female_middle`, `male_young`, `male_middle`],
  Hr = [`01`, `02`, `03`, `04`],
  Ur = [`class`, `casual`, `interview`, `internship`],
  Wr = {
    female_young: `女 · 青年`,
    female_middle: `女 · 中年`,
    male_young: `男 · 青年`,
    male_middle: `男 · 中年`,
  },
  Gr = { class: `上课`, casual: `休闲`, interview: `面试`, internship: `实习` };
function Kr(e = `female`) {
  return {
    bodyId: e === `male` ? `male_young` : `female_young`,
    frontHair: `01`,
    backHair: `01`,
    outfit: `class`,
  };
}
function qr(e) {
  if (!e || typeof e != `object`) return !1;
  let t = e;
  return (
    Object.keys(t).length === 4 &&
    Vr.includes(t.bodyId) &&
    Hr.includes(t.frontHair) &&
    Hr.includes(t.backHair) &&
    Ur.includes(t.outfit)
  );
}
var Jr = (e) => e.startsWith(`female`);
function Yr(e) {
  return [
    `char_${e.bodyId}_hair_back_${e.backHair}`,
    `char_${e.bodyId}_body`,
    `char_${e.bodyId}_outfit_${e.outfit}`,
    `char_${e.bodyId}_hair_front_${e.frontHair}`,
  ];
}
function Xr(e, t) {
  let n = 2166136261 ^ (e | 0);
  for (let e of `npc-look:${t}`)
    n = Math.imul(n ^ e.charCodeAt(0), 16777619) >>> 0;
  let r = (e, t) => e[(n >>> t) % e.length];
  return {
    bodyId: r(Vr, 0),
    frontHair: r(Hr, 5),
    backHair: r(Hr, 11),
    outfit: r(Ur, 17),
  };
}
var Zr = { x: 64, y: 16, size: 128 },
  Qr = { x: 352, y: 134, width: 256, height: 384 },
  $r = [`lamp`, `plant`, `cushion`],
  ei = (e) => `room_${e}`,
  ti = (e, t) => `furniture_${e}_${t}`,
  ni = (e, t, n, r, i, a = `placeholder`, o = !0) => ({
    src: e,
    width: t,
    height: n,
    alpha: r,
    pixelated: o,
    status: a,
    alt: i,
  }),
  ri = {};
for (let e of Vr) {
  let t = `assets/characters/${e}`;
  ri[`char_${e}_body`] = ni(`${t}/body.png`, 256, 384, !0, `${Wr[e]}素体`);
  for (let n of Hr)
    (ri[`char_${e}_hair_front_${n}`] = ni(
      `${t}/hair_front_${n}.png`,
      256,
      384,
      !0,
      `前发 ${n}`,
    )),
      (ri[`char_${e}_hair_back_${n}`] = ni(
        `${t}/hair_back_${n}.png`,
        256,
        384,
        !0,
        `后发 ${n}`,
      ));
  for (let n of Ur)
    ri[`char_${e}_outfit_${n}`] = ni(
      `${t}/outfit_${n}.png`,
      256,
      384,
      !0,
      `${Gr[n]}衣装`,
    );
}
var ii = { compact: `紧凑单间`, standard: `标准房间`, comfort: `舒适房间` };
for (let e of Un.map((e) => e.id)) {
  ri[ei(e)] = ni(
    `assets/rooms/room_${e}.png`,
    960,
    540,
    !1,
    `${ii[e]}：左侧书桌与窗，右侧床与收纳`,
  );
  for (let t of $r)
    ri[ti(e, t)] = ni(
      `assets/rooms/${e}/furniture_${t}.png`,
      960,
      540,
      !0,
      Vn.find((e) => e.id === t).name,
    );
}
for (let e of Rr)
  ri[e.cg] = ni(
    `assets/cg/events/${e.cg}.png`,
    960,
    540,
    !1,
    e.title.replaceAll(`{npc}`, `组员`),
  );
for (let e of Dn)
  ri[e.cg] = ni(`assets/cg/trips/${e.cg}.png`, 960, 540, !1, e.name);
(ri.opening_hong_kong = ni(
  `assets/cg/story/opening_hong_kong.png`,
  960,
  540,
  !1,
  `初抵香港：行李、城市与陌生的新生活`,
)),
  (ri.winter_family = ni(
    `assets/cg/winter/winter_family.png`,
    960,
    540,
    !1,
    `家中餐桌，返乡相聚`,
  )),
  (ri.internship_search = ni(
    `assets/cg/work/internship_search.png`,
    960,
    540,
    !1,
    `简历、电脑和求职准备的桌面`,
  )),
  (ri.internship_work = ni(
    `assets/cg/work/internship_work.png`,
    960,
    540,
    !1,
    `普通办公室工位`,
  )),
  (ri.ending_graduated = ni(
    `assets/cg/endings/ending_graduated.png`,
    960,
    540,
    !1,
    `校园与合上的学位证书`,
  )),
  (ri.ending_mainland_seeking = ni(
    `assets/cg/endings/ending_mainland_seeking.png`,
    960,
    540,
    !1,
    `回程车站与新的起点`,
  )),
  (ri.ending_phd = ni(
    `assets/cg/endings/ending_phd.png`,
    960,
    540,
    !1,
    `图书馆研究桌与学术资料`,
  ));
for (let e of Vn)
  ri[`item_${e.id}`] = ni(
    `assets/items/item_${e.id}.png`,
    128,
    128,
    !0,
    e.name,
  );
(ri.prototype_room = ni(
  `assets/room-v2.png`,
  1586,
  992,
  !1,
  `暖光照进香港的小出租屋，窗边有书桌，右侧是床`,
  `prototype`,
  !1,
)),
  (ri.prototype_female = ni(
    `assets/student-v2.png`,
    1024,
    1536,
    !0,
    `深色中长发、绿色开衫的主角（原型整图）`,
    `prototype`,
    !1,
  )),
  (ri.prototype_male = ni(
    `assets/student-male-v2.png`,
    1024,
    1536,
    !0,
    `深色短发、米色衬衫的主角（原型整图）`,
    `prototype`,
    !1,
  ));
for (let e of []) ri[e] && (ri[e].status = `approved`);
var ai = ri;
function oi(e, t = `/`) {
  let n = ai[e];
  return n
    ? (globalThis.__HK_INLINE_ASSETS__?.[n.src] ??
        `${t.endsWith(`/`) ? t : `${t}/`}${n.src}`)
    : null;
}
var si = (e) => !!ai[e] && ai[e].status !== 'placeholder', li = './', ui = (e) => ai[e]?.status === 'approved';
var gi = (e) => Object.fromEntries(N.map((t) => [t.id, structuredClone(e)])),
  _i = () => ({
    status: `unreleased`,
    released: null,
    progress: 0,
    polish: 0,
    submittedDay: null,
    late: !1,
    quality: null,
    sumA: 0,
    sumM: 0,
    halves: 0,
    score: null,
    legacy: !1,
  }),
  vi = () => ({
    ..._i(),
    members: [],
    roles: { research: null, writing: null, presentation: null },
    locked: !1,
    invites: {},
    shown: [],
    refreshed: !1,
    relation: `pending`,
  }),
  yi = () => Object.fromEntries(qn.map((e) => [e.id, 30])),
  bi = () => Object.fromEntries(Vn.map((e) => [e.id, 0])),
  xi = (e) => Math.max(0, Math.min(100, e)),
  R = (e) => structuredClone(e),
  Si = M.length - 1,
  Ci = (e) => M[e.round],
  wi = (e) => (M[e].kind === `winter` ? 21 : 28),
  Ti = (e) => (M[e].kind === `winter` ? [] : Array(28).fill(null));
function Ei(e, t) {
  let n = M[e];
  return n.kind === `winter`
    ? { day: n.startDay + t, half: null }
    : { day: n.startDay + Math.floor(t / 2), half: t % 2 == 0 ? `am` : `pm` };
}
function Di(e, t) {
  let { day: n, half: r } = Ei(e, t);
  return r ? ln(n, r) : null;
}
function Oi(e, t) {
  let n = Di(e, t);
  return n ? `${n.kind === `class` ? `class` : `exam`}:${n.course}` : null;
}
function ki(e) {
  let t = fn(e),
    n = $t(t.day);
  return {
    round: n.index,
    index: (t.day - n.startDay) * 2 + (t.half === `am` ? 0 : 1),
    ...t,
  };
}
function Ai(e, t, n) {
  let r = pn(t, n);
  return r &&
    e.exams[r.id]?.status === `excused` &&
    e.exams[r.id].makeup === `pending`
    ? r
    : null;
}
function ji(e, t, n) {
  let r = Oi(t, n);
  if (r || M[t].kind === `winter`) return r;
  let { day: i, half: a } = Ei(t, n),
    o = Ai(e, i, a);
  return o ? jn(o.id) : null;
}
var Mi = (e) => Ei(e.round, Math.min(e.cursor, wi(e.round) - 1)).day,
  z = (e) =>
    e.phase === `planning`
      ? 0
      : e.phase === `running`
        ? e.cursor
        : e.plan.length;
function B(e = 1) {
  let t = Sr(e);
  return {
    version: 9,
    jobShifts: 0,
    destination: null,
    ending: null,
    seed: e,
    npcs: t,
    affection: Object.fromEntries(t.map((e) => [e.id, or.start])),
    reputation: or.start,
    known: [],
    firstWalkDone: !1,
    event: null,
    events: [],
    initialAbilities: yi(),
    abilities: yi(),
    mastery: gi(0),
    jobs: bn(),
    reviewFocus: `research`,
    economy: null,
    housing: null,
    budget: 0,
    rentPaid: 0,
    deposit: 0,
    round: 0,
    phase: `planning`,
    cursor: 0,
    plan: Ti(0),
    lastPlan: null,
    money: 0,
    fatigue: F.startFatigue,
    stress: F.startStress,
    tasks: gi(_i()),
    projects: gi(vi()),
    attendance: gi({ attended: 0, excused: 0, expected: 0 }),
    exams: {},
    sickDay: null,
    notice: null,
    blockedReason: null,
    logs: [],
    history: [],
    name: `新同学`,
    appearance: Kr(`female`),
    inventory: bi(),
    installed: [],
    purchased: bi(),
    roundStart: { money: 0, fatigue: F.startFatigue, stress: F.startStress },
  };
}
function V(e) {
  if (!e || typeof e != `object`) return `能力分配格式错误。`;
  let t = qn.map((t) => e[t.id]);
  if (!t.every((e) => typeof e == `number` && Number.isInteger(e)))
    return `每项能力都须是整数。`;
  if (!t.every((e) => e >= 0 && e <= 100)) return `每项能力须在 0—100 之间。`;
  let n = t.reduce((e, t) => e + t, 0);
  return n === 120 ? null : `四项能力合计须恰好 120 点，当前 ${n} 点。`;
}
function Ni(e, t) {
  if (
    e.housing !== null ||
    e.round !== 0 ||
    e.history.length > 0 ||
    e.logs.length > 0
  )
    return { state: e, error: `开局能力只能在签约、开始生活之前分配。` };
  let n = V(t);
  if (n) return { state: e, error: n };
  let r = Object.fromEntries(qn.map((e) => [e.id, t[e.id]]));
  return {
    state: { ...R(e), initialAbilities: r, abilities: { ...r } },
    error: null,
  };
}
function Pi(e, t) {
  return qr(t)
    ? { state: { ...R(e), appearance: { ...t } }, error: null }
    : { state: e, error: `外观选择无效。` };
}
function Fi(e, t, n) {
  if (e.housing !== null)
    return { state: e, error: `已经签约，本版本不能中途换房。` };
  if (
    e.round !== 0 ||
    e.phase !== `planning` ||
    e.history.length > 0 ||
    e.logs.length > 0
  )
    return { state: e, error: `只能在开学前签约。` };
  let r = Hn.find((e) => e.id === t),
    i = Un.find((e) => e.id === n);
  if (!r || !i) return { state: e, error: `未知的经济档位或房型。` };
  let a = V(e.initialAbilities);
  if (a || qn.some((t) => e.abilities[t.id] !== e.initialAbilities[t.id]))
    return { state: e, error: a ?? `开局能力与分配记录不一致。` };
  let { annualRent: o, deposit: s } = Kn(n);
  if (o + s > r.budget)
    return {
      state: e,
      error: `预算不足：签约需要 HK$${o + s}（年租＋押金），初始预算只有 HK$${r.budget}。`,
    };
  let c = R(e);
  return (
    (c.economy = t),
    (c.housing = n),
    (c.budget = r.budget),
    (c.rentPaid = o),
    (c.deposit = s),
    (c.money = r.budget - o - s),
    (c.roundStart = { ...c.roundStart, money: c.money }),
    { state: c, error: null }
  );
}
function Ii(e) {
  return [...e.history, ...e.logs].filter((e) => e.half !== `am`).length;
}
function Li(e) {
  return (245 - Ii(e)) * F.dailyCost;
}
function Ri(e) {
  let t = e.housing ? Gn(e.housing) : Un[0],
    n = e.installed.includes(`cushion`) ? 3 : 0,
    r = e.installed.includes(`plant`) ? 2 : 0;
  return {
    fatigue: t.restFatigue + Math.min(n, Wn.fatigue),
    stress: t.restStress + Math.min(r, Wn.stress),
  };
}
function zi(e) {
  return e.housing ? Gn(e.housing).slots - e.installed.length : 0;
}
function Bi(e, t, n) {
  e.abilities[t] = Math.min(100, Math.round((e.abilities[t] + n) * 10) / 10);
}
function Vi(e, t, n) {
  e.mastery[t] = Math.min(100, Math.round((e.mastery[t] + n) * 10) / 10);
}
var Hi = (e, t) => `${Jn(e)} +${t}`;
function Ui(e) {
  let t = M[e.round].semester;
  return N.filter((e) => e.semester === t);
}
function Wi(e, t) {
  return Ui(e).some((e) => e.id === t) ? { ...R(e), reviewFocus: t } : e;
}
var Gi = (e) => `job:${e}`,
  Ki = (e) => (e?.startsWith(`job:`) ? e.slice(4) : null),
  qi = (e) => e.filter((e) => Ki(e) !== null).length;
function Ji(e) {
  return (
    e.housing !== null &&
    M[e.round].kind === `semester` &&
    e.phase === `planning`
  );
}
function Yi(e, t, n) {
  if (!Ji(e))
    return {
      state: e,
      error: `只能在普通轮开始前的日程规划阶段求职；寒假不参加普通求职。`,
      attempt: null,
    };
  let r = Sn(e.jobs, e.round, t, e.abilities, n);
  return r.error
    ? { state: e, error: r.error, attempt: null }
    : { state: { ...R(e), jobs: r.market }, error: null, attempt: r.attempt };
}
function Xi(e, t) {
  return gn(t)
    ? e.jobs.hired.includes(t)
      ? null
      : `尚未获得这个岗位；请先在“兼职求职”中申请并获录用。`
    : `未知岗位。`;
}
function Zi(e, t, n = `lower`) {
  let r = (t) => ({ plan: e.plan, slots: [], preview: null, error: t }),
    i = gn(t);
  if (!i) return r(`未知岗位。`);
  if (
    M[e.round].kind !== `semester` ||
    (e.phase !== `planning` && e.phase !== `running`)
  )
    return r(`只能在普通轮安排或暂停日程时替换兼职。`);
  if (!e.jobs.hired.includes(t))
    return r(`目标岗位尚未录用；只有已录用的岗位才能换进日程。`);
  if (n === t) return r(`来源岗位与目标岗位相同。`);
  let a = [];
  for (let r = z(e); r < e.plan.length; r++) {
    let o = Ki(e.plan[r]);
    !o ||
      o === t ||
      ((n === `lower` ? gn(o).pay < i.pay : n === `others` || o === n) &&
        a.push(r));
  }
  if (!a.length)
    return r(
      n === `lower`
        ? `本轮尚未执行的日程里，没有工资低于「${i.name}」的兼职半日。`
        : n === `others`
          ? `本轮尚未执行的日程里，没有其他岗位的兼职半日。`
          : `本轮尚未执行的日程里，没有「${gn(n)?.name ?? `该岗位`}」的兼职半日。`,
    );
  let o = [...e.plan],
    s = { count: a.length, pay: 0, fatigue: 0, stress: 0, growth: {} },
    c = (e, t) => {
      s.growth[e] = Math.round(((s.growth[e] ?? 0) + t) * 10) / 10;
    };
  for (let e of a) {
    let n = gn(Ki(o[e]));
    (s.pay += i.pay - n.pay),
      (s.fatigue += i.fatigue - n.fatigue),
      (s.stress += i.stress - n.stress),
      c(n.growth, -n.growthPerShift),
      c(i.growth, i.growthPerShift),
      (o[e] = Gi(t));
  }
  for (let e of Object.keys(s.growth)) s.growth[e] === 0 && delete s.growth[e];
  return { plan: o, slots: a, preview: s, error: null };
}
var Qi = (e) =>
    e.expected > 0 && (e.attended + e.excused) * 10 < e.expected * 7,
  $i = (e) => (e.expected === 0 ? null : (e.attended + e.excused) / e.expected);
function ea(e, t, n, r) {
  let i = Fn,
    a =
      i.penaltyRate * Math.max(n - i.penaltyFrom, 0) +
      i.penaltyRate * Math.max(r - i.penaltyFrom, 0);
  return (
    Math.round(
      Math.max(0, Math.min(100, i.base + i.mastery * e + i.research * t - a)) *
        100,
    ) / 100
  );
}
var ta = (e) => dn.find((t) => t.id === e) ?? null,
  na = (e) =>
    `${$n(e.course)}${e.kind === `midterm` ? `期中考试` : `期末考试`}`;
function ra(e, t) {
  return dn.find((n) => n.course === e && n.kind === t) ?? null;
}
var ia = (e) => Math.round(e * 10) / 10,
  H = (e, t) => zn.pointsPerUnit * Tn(t)[e].workloadUnits,
  aa = (e) => zn.progress.scale * (zn.progress.base + e / zn.progress.divisor),
  oa = (e, t) =>
    ia(
      Math.max(
        0,
        Math.min(
          zn.quality.max,
          zn.quality.base + zn.quality.ability * e + zn.quality.mastery * t,
        ),
      ),
    ),
  sa = (e) => zn.polish[e - 1] ?? zn.polishAfter,
  ca = (e) =>
    e.quality === null
      ? 0
      : Math.max(
          0,
          ia(Math.min(zn.cap, ia(e.quality + sa(e.polish + 1))) - e.quality),
        ),
  la = (e) => (e === `individual` ? `个人作业` : `小组项目`);
function ua(e, t, n) {
  return t === `individual` ? e.tasks[n] : e.projects[n];
}
function da(e, t) {
  return En(e, Tn(e)[t].dueWeek);
}
function fa(e, t) {
  return da(e, t) + 14;
}
var pa = (e, t) => e.npcs.find((e) => e.id === t) ?? null;
function ma(e) {
  let t = [`self`, ...e.members];
  return (
    Array.isArray(e.members) &&
    e.members.length === 2 &&
    new Set(e.members).size === 2 &&
    !!e.roles &&
    Bn.every((n) => e.roles[n.id] !== null && t.includes(e.roles[n.id])) &&
    new Set(Object.values(e.roles)).size === 3
  );
}
function ha(e, t, n, r) {
  let i = pr(t, r);
  return n === `self` ? e.abilities[i] : pa(e, n).abilities[i];
}
function ga(e, t, n) {
  if (t === `individual`) return e.abilities[Yn[n]];
  let r = e.projects[n];
  return Bn.reduce((t, i) => t + ha(e, n, r.roles[i.id], i.id), 0) / Bn.length;
}
function _a(e, t, n) {
  if (t === `individual`) return ia(aa(e.abilities[Yn[n]]));
  let r = e.projects[n];
  return ma(r)
    ? ia(
        Bn.reduce((t, i) => t + aa(ha(e, n, r.roles[i.id], i.id)), 0) /
          Bn.length,
      )
    : 0;
}
var va = (e, t) => _a(e, `group`, t);
function ya(e) {
  return e.status === `missing` ? 0 : e.status === `submitted` ? e.score : null;
}
function ba(e) {
  let t =
    e.status === `submitted` || e.status === `missing` ? ya(e) : e.quality;
  return t === null ? null : Ar(t).letter;
}
function xa(e, t) {
  let n = M.filter((e) => e.semester === t).at(-1).index;
  return (
    e.round > n ||
    (e.round === n && (e.phase === `summary` || e.phase === `finished`))
  );
}
var Sa = (e) => N.find((t) => t.id === e).semester,
  Ca = (e) => (Math.floor(e * 10 + 1e-9) / 10).toFixed(1),
  wa = {
    individual: `个人作业`,
    group: `小组项目`,
    midterm: `期中考试`,
    final: `期末考试`,
  },
  Ta = (e) => wa[e];
function Ea(e, t) {
  let n = wn(t).type,
    r = Cr[n],
    i = {},
    a = [],
    o = [];
  for (let n of [`individual`, `group`]) {
    let r = ua(e, n, t),
      s = ya(r);
    s === null
      ? r.legacy && r.status === `submitted`
        ? o.push(n)
        : a.push(n)
      : (i[n] = s);
  }
  for (let n of [`midterm`, `final`]) {
    if (!(n in r)) continue;
    let s = e.exams[ra(t, n).id];
    s && s.score !== null
      ? (i[n] = s.score)
      : s?.legacy
        ? o.push(n)
        : a.push(n);
  }
  let s = kr(n, i),
    c = Object.entries(i).reduce((e, [t, n]) => e + n * r[t], 0) / 100,
    l = xa(e, Sa(t)),
    u = e.attendance[t],
    d = Qi(u),
    f = l && d,
    p = `in_progress`,
    m = null;
  l && (f || (o.length === 0 && s !== null))
    ? ((p = `official`), (m = jr(n, i, f)))
    : l && (p = `unknown`);
  let h = m?.status === `graded` ? m : null,
    g = [];
  if (
    (f
      ? g.push(
          `普通课堂出勤率 ${Math.round($i(u) * 1e3) / 10}% 低于 70%，课程强制 F、绩点 0`,
        )
      : h && !h.passed && g.push(`课程总分 ${Ca(s)} 低于及格线 50`),
    h && !h.passed)
  )
    for (let n of Object.keys(r))
      i[n] === 0 &&
        (!(n === `individual` || n === `group`) ||
          ua(e, n, t).status === `missing`) &&
        g.push(
          `${Ta(n)}${n === `individual` || n === `group` ? `缺交` : `缺考`}，0 分按原权重 ${r[n]}% 计入`,
        );
  let _ = h ? h.letter : s === null ? null : Ar(s).letter;
  return {
    type: n,
    weights: r,
    scores: i,
    pending: a,
    unknown: o,
    total: s,
    counted: c,
    closed: l,
    status: p,
    result: h,
    letter: _,
    attendanceRisk: d,
    forcedFail: f,
    reasons: g,
    legacy: o.length > 0,
  };
}
function Da(e, t) {
  let n = N.filter((e) => e.semester === t).map((t) => ({
      course: t.id,
      ...Ea(e, t.id),
    })),
    r = n.flatMap((e) => (e.result ? [e.result] : []));
  return {
    semester: t,
    closed: xa(e, t),
    courses: n,
    gpa: Nr(r),
    complete: n.every((e) => e.status === `official`),
  };
}
function Oa(e) {
  let t = N.map((t) => ({ course: t.id, ...Ea(e, t.id) })),
    n = t.map((e) => e.result ?? { status: `in_progress` }),
    r = t.flatMap((e) => (e.result ? [e.result] : [])),
    i = r.reduce((e, t) => e + t.points100, 0);
  return {
    courses: t,
    cgpa: Nr(r),
    sum100: i,
    count: r.length,
    graduation: Fr(n),
    complete: t.every((e) => e.status === `official`),
    unknown: t.some((e) => e.status === `unknown`),
  };
}
function ka(e) {
  let t =
    (e.abilities.writing + e.abilities.communication + e.abilities.practice) /
    3;
  return {
    avg: t,
    abilityOk: t + 1e-9 >= Ln.employAbilityAvg,
    jobShifts: e.jobShifts,
    internship: !1,
    experienceOk: e.jobShifts >= Ln.employJobShifts || !1,
    ok:
      t + 1e-9 >= Ln.employAbilityAvg &&
      (e.jobShifts >= Ln.employJobShifts || !1),
  };
}
var Aa = N.filter((e) => Yn[e.id] === `research`).map((e) => e.id);
function ja(e, t) {
  let n = Oa(e),
    r = n.graduation.status === `graduated`,
    i = ka(e),
    a = [{ label: `顺利毕业（八门课全部及格，且 CGPA ≥ 2.00）`, met: r }];
  if (t === `phd`) {
    let t = n.count === N.length && n.sum100 >= Ln.phdCgpa100 * n.count,
      r = Aa.some((e) => {
        let t = n.courses.find((t) => t.course === e).result;
        return (
          !!t &&
          t.passed &&
          !t.forcedFail &&
          t.total !== null &&
          t.total + 1e-9 >= Ln.phdCourseMin
        );
      });
    a.push(
      { label: `CGPA ≥ 3.30`, met: t },
      {
        label: `研究分析 ≥ ${Ln.phdResearch}（当前 ${e.abilities.research.toFixed(1)}）`,
        met: e.abilities.research >= Ln.phdResearch,
      },
      {
        label: `写作表达 ≥ ${Ln.phdWriting}（当前 ${e.abilities.writing.toFixed(1)}）`,
        met: e.abilities.writing >= Ln.phdWriting,
      },
      {
        label: `至少一门研究方法类课程（${Aa.map($n).join(`／`)}）正式总评 ≥ 80（A− 或以上）`,
        met: r,
      },
    );
  } else
    a.push(
      {
        label: `写作、沟通、实践三项平均 ≥ ${Ln.employAbilityAvg}（当前 ${Ca(i.avg)}）`,
        met: i.abilityOk,
      },
      {
        label: `累计完成兼职 ≥ ${Ln.employJobShifts} 个半日（当前 ${i.jobShifts}）或完成实习（实习尚未开放）`,
        met: i.experienceOk,
      },
    ),
      t === `hk` &&
        a.push({ label: `或：获得香港实习留用机会（实习尚未开放）`, met: !1 });
  return a;
}
function Ma(e, t) {
  return In.some((e) => e.id === t)
    ? e.ending
      ? { state: e, error: `学年结局已经确认。` }
      : M[e.round].semester === 2
        ? { state: { ...R(e), destination: t }, error: null }
        : { state: e, error: `第二学期开始后才能选择去向志愿。` }
    : { state: e, error: `未知的去向。` };
}
var Na = (e) =>
    e.status !== `unreleased` &&
    e.status !== `submitted` &&
    e.status !== `missing`,
  Pa = (e) => N.findIndex((t) => t.id === e);
function Fa(e, t) {
  let n = e.projects[t];
  return yr(
    e.npcs.filter((e) => e.kind === `pool`).map((e) => e.id),
    e.seed * 31 + Pa(t),
  ).filter((e) => !n.members.includes(e) && !(e in n.invites));
}
function Ia(e, t, n) {
  return Fa(e, t)
    .filter((e) => !n.includes(e))
    .slice(0, 4);
}
function La(e, t) {
  let n = e.projects[t];
  return n
    ? n.status === `unreleased`
      ? `老师尚未发布这个小组项目。`
      : Na(n)
        ? n.locked
          ? `已经实际进行过小组作业，成员与分工已锁定。`
          : null
        : `这个小组项目已经结束。`
    : `未知课程。`;
}
function Ra(e) {
  return Object.keys(e.invites).length;
}
function za(e, t) {
  let n = e.projects[t],
    r = [`self`, ...n.members],
    i = n.roles,
    a = -1;
  for (let n of r)
    for (let o of r)
      for (let s of r) {
        if (new Set([n, o, s]).size !== 3) continue;
        let r = { research: n, writing: o, presentation: s },
          c = Bn.reduce((n, i) => n + ha(e, t, r[i.id], i.id), 0);
        c > a && ((a = c), (i = r));
      }
  return i;
}
function Ba(e, t) {
  let n = e.projects[t];
  n.roles =
    n.members.length === 2
      ? za(e, t)
      : { research: null, writing: null, presentation: null };
}
function Va(e, t) {
  let n = La(e, t);
  if (n) return { state: e, error: n };
  let r = e.projects[t];
  if (r.members.length >= 2) return { state: e, error: `队伍已经满员。` };
  if (r.refreshed) return { state: e, error: `这个项目的额外刷新已经用过了。` };
  let i = Ia(e, t, r.shown);
  if (!i.length) return { state: e, error: `没有更多可以邀请的候选同学了。` };
  let a = R(e);
  return (
    (a.projects[t].shown = i),
    (a.projects[t].refreshed = !0),
    (a.known = [...new Set([...a.known, ...i])]),
    { state: a, error: null }
  );
}
function Ha(e, t) {
  let n = pa(e, t);
  return n ? fr(dr(n.abilities), e.affection[t], e.reputation) : null;
}
function Ua(e, t, n, r) {
  let i = (t) => ({ state: e, error: t, success: null, chance: null }),
    a = La(e, t);
  if (a) return i(a);
  let o = e.projects[t],
    s = pa(e, n);
  if (!s || s.kind !== `pool`) return i(`只能正式邀请候选同学。`);
  if (!e.known.includes(n)) return i(`还没有认识这位同学。`);
  if (o.members.includes(n)) return i(`这位同学已经在本组。`);
  if (n in o.invites) return i(`本项目已经正式邀请过这位同学，不能重复邀请。`);
  if (o.members.length >= 2) return i(`队伍已经满员。`);
  if (!Number.isFinite(r) || r < 0 || r >= 1)
    throw RangeError(`随机数必须在 [0,1)：${r}`);
  let c = Ha(e, n),
    l = r < c / 100,
    u = R(e),
    d = u.projects[t];
  return (
    (d.invites[n] = l),
    l && (d.members.push(n), Ba(u, t)),
    { state: u, error: null, success: l, chance: c }
  );
}
function Wa(e, t) {
  if (La(e, t)) return !1;
  let n = e.projects[t];
  return n.members.length >= 2 ? !1 : Ra(n) >= 2 || Fa(e, t).length === 0;
}
function Ga(e, t) {
  if (!Wa(e, t)) {
    let n = e.projects[t];
    return {
      state: e,
      error:
        La(e, t) ??
        (n.members.length >= 2
          ? `队伍已经满员。`
          : `至少正式邀请 2 次后仍缺人，才能公开招募。`),
      added: [],
    };
  }
  let n = R(e),
    r = n.projects[t],
    i = [];
  for (let e of n.npcs.filter((e) => e.kind === `fallback`)) {
    if (r.members.length >= 2) break;
    r.members.includes(e.id) || (r.members.push(e.id), i.push(e.id));
  }
  return (
    Ba(n, t),
    (n.known = [...new Set([...n.known, ...i])]),
    { state: n, error: null, added: i }
  );
}
function Ka(e, t, n) {
  let r = La(e, t);
  if (r) return { state: e, error: r };
  let i = e.projects[t];
  if (i.members.length < 2)
    return { state: e, error: `队伍满员后才能分配岗位。` };
  if (!ma({ members: i.members, roles: n }))
    return { state: e, error: `三人各负责一个不同岗位。` };
  let a = R(e);
  return (a.projects[t].roles = { ...n }), { state: a, error: null };
}
var qa = (e) => (e > 0 ? `+${e}` : `${e}`);
function Ja(e, t) {
  let n = e.projects[t];
  if (
    n.relation !== `pending` ||
    (n.status !== `submitted` && n.status !== `missing`)
  )
    return ``;
  if (e.event?.course === t)
    return ` 本项目的交付关系将在小组事件选择之后结算。`;
  let r = n.status === `missing` ? `missing` : n.late ? `late` : `on_time`,
    i = sr[r],
    a = r === `missing` ? null : Ar(n.score).letter,
    o = e.reputation;
  for (let t of n.members) e.affection[t] = ur(e.affection[t] + i.affection);
  (e.reputation = gr(e.reputation, r, a)), (n.relation = `graded`);
  let s =
    n.members.length === 0
      ? ``
      : n.members.length === 1
        ? `组员好感 ${qa(i.affection)}、`
        : `${n.members.length} 位组员好感各 ${qa(i.affection)}、`;
  return ` ${N[Pa(t)].name}小组项目${r === `on_time` ? `按时提交` : r === `late` ? `迟交提交` : `缺交`}${a ? `（${a.replace(`-`, `−`)}）` : ``}：${s}声望 ${qa(e.reputation - o)}。`;
}
function Ya(e, t, n, r, i) {
  let a = ua(e, t, n);
  return (
    (a.status = `submitted`),
    (a.submittedDay = r),
    (a.late = i),
    (a.score = ia(Ir(i ? `late` : `on_time`, a.quality))),
    t === `group` ? Ja(e, n) : ``
  );
}
function Xa(e, t, n) {
  let r = ua(e, t, n);
  return (r.status = `missing`), (r.score = 0), t === `group` ? Ja(e, n) : ``;
}
function Za(e, t, n) {
  let r = N.find((e) => e.id === n);
  if (!r) return `未知课程。`;
  let i = ua(e, t, n),
    a = la(t);
  return i.status === `unreleased`
    ? t === `individual`
      ? `${r.name}的个人作业尚未布置；个人作业在第 3 教学周的课堂时段发布。`
      : `${r.name}的小组项目尚未发布；小组项目在第 4 教学周的课堂时段发布。`
    : i.status === `submitted`
      ? `${r.name}的${a}已提交，不能再修改或打磨。`
      : i.status === `missing`
        ? `${r.name}的${a}补交期已过，记为缺交（0 分），不能再做。`
        : t === `group` && !ma(e.projects[n])
          ? `请先在“小组项目”中为${r.name}凑齐两位组员（邀请或公开招募），并为三人分配岗位。`
          : null;
}
function Qa(e, t) {
  let n = Mn(t);
  if (n)
    return e.exams[n]?.makeup === `pending` ? null : `这场补考已经不需要安排。`;
  let r = P(t);
  if (r && !Dn.some((e) => e.id === r)) return `未知地点。`;
  let i = rr(t);
  if (i) return Za(e, i.kind, i.course);
  let a = Ki(t);
  return a ? Xi(e, a) : null;
}
function $a(e, t, n) {
  if (e.phase !== `planning` && e.phase !== `running`)
    return { state: e, error: `请在安排或暂停日程时提交。` };
  let r = ua(e, t, n);
  if (!r) return { state: e, error: `未知课程。` };
  if (r.status !== `complete`)
    return {
      state: e,
      error:
        r.status === `submitted`
          ? `这份作业已经提交过了。`
          : `作业完成后才能提交。`,
    };
  if (e.event) return { state: e, error: `请先完成当前事件的选择。` };
  let i = R(e);
  return Ya(i, t, n, Mi(i), !1), { state: i, error: null };
}
function eo(e, t) {
  let n = t - (t % 2);
  return P(e.plan[t]) !== null && n < z(e) && P(e.plan[n]) === P(e.plan[t]);
}
function to(e, t) {
  return !ji(e, e.round, t) && !ji(e, e.round, t + 1);
}
function no(e, t, n) {
  if (!Number.isInteger(t) || t < z(e) || t >= e.plan.length)
    return { plan: e.plan, error: `只能修改尚未执行的半日。` };
  if (eo(e, t))
    return { plan: e.plan, error: `全天游玩已经出发，下午不能改成别的安排。` };
  if (P(n)) {
    let r = t - (t % 2);
    if (r < z(e))
      return {
        plan: e.plan,
        error: `全天游玩要占用同一天上午和下午两个尚未执行的半日。`,
      };
    if (!to(e, r))
      return {
        plan: e.plan,
        error: `这一天有课程、考试或补考，不能安排全天游玩。`,
      };
    let i = Qa(e, n);
    if (i) return { plan: e.plan, error: i };
    let a = [...e.plan];
    return (a[r] = n), (a[r + 1] = n), { plan: a, error: null };
  }
  if (
    (n?.startsWith(`class:`) || n?.startsWith(`exam:`)) &&
    n !== Oi(e.round, t)
  )
    return { plan: e.plan, error: `课堂和考试只能安排在课表指定的时段。` };
  if (Mn(n) && n !== ji(e, e.round, t))
    return { plan: e.plan, error: `补考只能安排在指定的补考时段。` };
  let r = Qa(e, n);
  if (r) return { plan: e.plan, error: r };
  let i = er(n);
  if (i && n !== e.plan[t] && e.plan.filter((e) => e === n).length >= i.cap)
    return { plan: e.plan, error: `${i.name}每轮最多 ${i.cap} 个半日。` };
  if (Ki(n) && Ki(e.plan[t]) === null && qi(e.plan) >= 6)
    return { plan: e.plan, error: `所有兼职合计每轮最多 6 个半日。` };
  let a = [...e.plan];
  if (P(a[t])) {
    let e = t - (t % 2);
    (a[e] = null), (a[e + 1] = null);
  }
  return (a[t] = n), { plan: a, error: null };
}
function ro(e) {
  let t = [...e.plan],
    n = 0,
    r = [];
  for (let i = z(e); i < t.length; i++) {
    let a = ji(e, e.round, i);
    !a || t[i] === a || (t[i] === null ? ((t[i] = a), n++) : r.push(i));
  }
  return { plan: t, added: n, conflicts: r };
}
function io(e, t, n, r = Array.from({ length: e.plan.length }, (e, t) => t)) {
  let i = Qa(e, t),
    a = [...e.plan];
  if (i) return { plan: a, added: 0, unfilled: n, error: i };
  let o = er(t),
    s = Ki(t)
      ? 6 - qi(a)
      : o
        ? o.cap - a.filter((e) => e === t).length
        : a.length,
    c = Math.max(0, Math.min(Math.floor(n), s)),
    l = 0;
  for (let n of [...new Set(r)].sort((e, t) => e - t))
    !Number.isInteger(n) ||
      n < z(e) ||
      n >= a.length ||
      a[n] !== null ||
      l >= c ||
      ((a[n] = t), l++);
  return { plan: a, added: l, unfilled: Math.max(0, n - l), error: null };
}
function ao(e) {
  let t = R(e),
    n = 0,
    r = 0;
  if (!e.lastPlan || e.lastPlan.length !== e.plan.length)
    return { plan: t.plan, added: n, skipped: r };
  for (let i = z(e); i < e.plan.length; i++) {
    let a = e.lastPlan[i];
    if (t.plan[i] !== null || !a || (P(a) && i % 2 == 1)) continue;
    if (P(a) && t.plan[i + 1] !== null) {
      r++;
      continue;
    }
    let o = no(t, i, a);
    o.error ? r++ : ((t.plan = o.plan), n++);
  }
  return { plan: t.plan, added: n, skipped: r };
}
function oo(e, t) {
  if (t.length !== e.plan.length) return e;
  let n = R(e),
    r = z(e);
  for (let i = r; i < t.length; i++) n.plan[i] = eo(e, i) ? e.plan[i] : t[i];
  for (let t = r; t < n.plan.length; t++) {
    let i = P(n.plan[t]);
    if (!i || eo(e, t)) continue;
    let a = t - (t % 2);
    (a >= r && P(n.plan[a]) === i && P(n.plan[a + 1]) === i && to(e, a)) ||
      (n.plan[t] = null);
  }
  return (n.blockedReason = null), n;
}
function so(e) {
  return e.phase !== `planning` || e.plan.some((e) => e === null)
    ? e
    : {
        ...R(e),
        phase: `running`,
        cursor: 0,
        logs: [],
        roundStart: { money: e.money, fatigue: e.fatigue, stress: e.stress },
      };
}
function co(e) {
  return { ...R(e), notice: null };
}
var lo = (e) =>
  M[e].kind === `winter`
    ? null
    : e - M.slice(0, e).filter((e) => e.kind === `winter`).length;
function U(e) {
  return e.events.filter((t) => t.round === e.round).length;
}
function W(e, t) {
  let n = lo(e.round);
  return (
    n === null ||
    e.events.some((e) => e.id === t && n - lo(e.round) <= Lr.cooldownRounds)
  );
}
function uo(e) {
  return !e.firstWalkDone && e.plan.slice(e.cursor).includes(`walk`) ? 1 : 0;
}
function fo(e, t, n, r, i = null, a = null) {
  (e.event = { id: t, day: n, half: r, course: i, npc: a }),
    e.events.push({
      id: t,
      round: e.round,
      day: n,
      choice: null,
      course: i,
      npc: a,
    });
}
var po = (e) => Br(e).trigger;
function mo(e, t) {
  if (U(e) + uo(e) >= Lr.perRoundCap) return !1;
  let n = Lr.categoryCap[t];
  return (
    n === void 0 ||
    e.events.filter((n) => n.round === e.round && po(n.id) === t).length < n
  );
}
function ho(e, t, n, r, i, a = () => !0) {
  if (!mo(e, t)) return null;
  let o = Rr.filter((n) => n.enabled && n.trigger === t && !W(e, n.id) && a(n));
  return !o.length || vr(e.seed, r, i, 1) >= n
    ? null
    : o[Math.floor(vr(e.seed, r, i, 2) * o.length)];
}
function go(e, t, n) {
  if (!e.firstWalkDone) {
    (e.firstWalkDone = !0), fo(e, Lr.firstWalkEvent, t, n);
    return;
  }
  let r = ho(e, `walk`, Lr.walkChance, t, n === `am` ? 0 : 1);
  r && fo(e, r.id, t, n);
}
function _o(e, t, n, r, i) {
  let a = e.projects[t];
  if (!a.members.length) return;
  let o = ho(
    e,
    `group`,
    Lr.groupChance,
    n,
    r === `am` ? 4 : 5,
    (e) => e.stage === i,
  );
  if (!o) return;
  let s =
    a.members[
      Math.floor(vr(e.seed, n, r === `am` ? 4 : 5, 3) * a.members.length)
    ];
  fo(e, o.id, n, r, t, s);
}
function vo(e, t, n) {
  if (e.sickDay === t || e.events.some((e) => e.day === t)) return;
  let r = ho(e, `daily`, Lr.dailyChance, t, 3, (e) => !e.studied || n);
  r && fo(e, r.id, t, `pm`);
}
function yo(e, t) {
  if (!e.event) return { state: e, error: `没有待选择的事件。` };
  let n = Br(e.event.id),
    r = n?.options[t];
  if (!n || !r) return { state: e, error: `没有这个选项。` };
  if (r.money < 0 && e.money < -r.money)
    return { state: e, error: `余额不足，请选择其他选项。` };
  let i = R(e),
    a = { money: i.money, fatigue: i.fatigue, stress: i.stress };
  (i.money += r.money),
    (i.fatigue = xi(i.fatigue + r.fatigue)),
    (i.stress = xi(i.stress + r.stress));
  let o = e.event.npc,
    s = o ? (pa(i, o)?.name ?? ``) : ``;
  r.affection && o && (i.affection[o] = ur(i.affection[o] + r.affection)),
    r.reputation && (i.reputation = ur(i.reputation + r.reputation)),
    (i.events[i.events.length - 1].choice = t),
    (i.event = null);
  let c = e.event.course,
    l = c ? Ja(i, c) : ``,
    u = i.logs.at(-1);
  return (
    u &&
      ((u.text += ` 【${n.title.replaceAll(`{npc}`, s)}】${r.label} —— ${r.result.replaceAll(`{npc}`, s)}${r.affection && o ? `（${s}好感 ${r.affection > 0 ? `+` : ``}${r.affection}）` : ``}${r.reputation ? `（声望 ${r.reputation > 0 ? `+` : ``}${r.reputation}）` : ``}`),
      (u.text += l),
      (u.money += i.money - a.money),
      (u.fatigue += i.fatigue - a.fatigue),
      (u.stress += i.stress - a.stress)),
    { state: i, error: null }
  );
}
function bo(e, t) {
  return e.fatigue < F.illness && e.stress < F.illness
    ? !1
    : ((e.sickDay = t),
      (e.money -= F.medical),
      (e.fatigue -= F.illnessFatigueRelief),
      (e.stress -= F.illnessStressRelief),
      !0);
}
function xo(e) {
  return (
    (e.money -= F.dailyCost),
    (e.fatigue -= F.dailyRecovery),
    (e.stress -= F.dailyCalm),
    ``
  );
}
function So(e, t, n) {
  let r = t.course,
    i = N.find((e) => e.id === r),
    a = Tn(r);
  return t.week === a.individual.releaseWeek &&
    e.tasks[r].status === `unreleased`
    ? ((e.tasks[r] = { ...e.tasks[r], status: `active`, released: n }),
      (e.notice = { course: r, kind: `individual` }),
      ` ${i.teacher}布置了个人作业「${i.assignment}」，${I(da(r, `individual`))}（第 ${a.individual.dueWeek} 周末）截止。`)
    : t.week === a.group.releaseWeek && e.projects[r].status === `unreleased`
      ? ((e.projects[r] = { ...e.projects[r], status: `active`, released: n }),
        (e.projects[r].shown = Ia(e, r, [])),
        (e.known = [...new Set([...e.known, ...e.projects[r].shown])]),
        (e.notice = { course: r, kind: `group` }),
        ` ${i.teacher}发布了小组项目，${I(da(r, `group`))}（第 ${a.group.dueWeek} 周末）截止，请先邀请组员并分工。`)
      : ``;
}
function Co(e, t) {
  let n = ``;
  for (let r of N)
    for (let i of [`individual`, `group`]) {
      let a = ua(e, i, r.id),
        o = da(r.id, i),
        s = `${r.name}${la(i)}`;
      if (a.status === `complete` && t >= o) {
        let t = Ya(e, i, r.id, o, !1);
        n += ` ${s}已在截止日自动提交，得分 ${a.score}（${ba(a).replace(`-`, `−`)}）。${t}`;
      } else
        a.status === `active` &&
          t >= o &&
          ((a.status = `overdue`),
          (n += ` ${s}未能在截止日前完成，进入补交期：${I(o + 14)}前完成会立即迟交提交，得分为原始分 × 0.8。`));
      if (a.status === `overdue` && t >= o + 14) {
        let t = Xa(e, i, r.id);
        n += ` ${s}补交期结束仍未完成，记为缺交（0 分，按原权重计入课程总评）。${t}`;
      }
    }
  return n;
}
function wo(e, t, n, r, i) {
  let a = ua(e, t, n),
    o = N.find((e) => e.id === n),
    s = Rn[t],
    c = t === `individual` ? `「${o.assignment}」` : `${o.name}小组项目`,
    l = ga(e, t, n),
    u = e.mastery[n],
    d = a.status !== `complete`;
  (e.fatigue += s.fatigue), (e.stress += s.stress);
  let f = ``,
    p = !1;
  if (a.status === `complete`) {
    let e = ca(a);
    a.polish++,
      (a.quality = ia(a.quality + e)),
      (f =
        e > 0
          ? `继续打磨${c}（第 ${a.polish} 次）：质量 +${e}，现在 ${a.quality}（预计 ${ba(a).replace(`-`, `−`)}）。`
          : `继续打磨${c}（第 ${a.polish} 次）：质量已达上限 ${zn.cap}，不再提高。`);
  } else {
    let r = H(t, n),
      i = _a(e, t, n);
    (a.progress = Math.min(r, ia(a.progress + i))),
      (a.sumA = ia(a.sumA + l)),
      (a.sumM = ia(a.sumM + u)),
      a.halves++,
      a.progress < r
        ? (f =
            t === `individual`
              ? `推进${c}，进度 +${i}，当前 ${a.progress}/${r}。`
              : `三人按分工协作，${c}进度 +${i}，当前 ${a.progress}/${r}。`)
        : ((a.quality = oa(a.sumA / a.halves, a.sumM / a.halves)),
          a.status === `overdue`
            ? (p = !0)
            : ((a.status = `complete`),
              (f = `完成${c}！作品质量 ${a.quality}（预计 ${ba(a).replace(`-`, `−`)}）。可以在截止前手动提交，也可以继续打磨；未手动提交的会在 ${I(da(n, t))} 截止日结束时自动提交。`)));
  }
  if (t === `individual`)
    Bi(e, `writing`, Xn.study.ability),
      Vi(e, n, Xn.study.mastery),
      (f += ` ${Hi(`writing`, Xn.study.ability)}，${o.name}掌握度 +${Xn.study.mastery}。`),
      e.installed.includes(`lamp`) && (e.fatigue -= 2);
  else {
    let t = e.projects[n],
      s = Bn.find((e) => t.roles[e.id] === `self`).id,
      c = pr(n, s);
    t.locked || ((t.locked = !0), (f += ` 小组已开工，分工从现在起锁定。`)),
      Bi(e, c, Xn.group.ability),
      Vi(e, n, Xn.group.mastery),
      (f += ` 你负责${mr(n, s)}：${Hi(c, Xn.group.ability)}，${o.name}掌握度 +${Xn.group.mastery}。`),
      _o(
        e,
        n,
        r,
        i,
        d && !p && a.status !== `complete` ? `production` : `finished`,
      );
  }
  if (p) {
    let i = Ya(e, t, n, r, !0);
    f =
      `在补交期内完成${c}，作品质量 ${a.quality}，已自动迟交提交：得分 ${a.score}（${ba(a).replace(`-`, `−`)}）。${i}` +
      f;
  }
  return { title: `${s.name} · ${o.name}`, text: f };
}
function To(e, t, n) {
  let r = ki(t.id),
    i = ` ${na(t)}因病假缺考，不按 0 分处理：补考安排在 ${I(r.day)}${Qn[r.half]}，与正式考试同一公式计分。`;
  if (r.round === e.round && r.index > n) {
    let n = e.plan[r.index];
    if (P(n)) {
      let t = r.index - (r.index % 2);
      (e.plan[t] = null), (e.plan[t + 1] = null);
    }
    (e.plan[r.index] = jn(t.id)),
      (e.notice = { course: t.course, kind: `makeup` }),
      n &&
        n !== e.plan[r.index] &&
        (i += `那个半日原来的「${ir(n)}」已改为补考${P(n) ? `；同一天的全天游玩已取消，另一个半日请重新安排` : ``}。`);
  }
  return i;
}
function Eo(e) {
  let t = Ei(e.round, e.cursor).day,
    n = { money: e.money, fatigue: e.fatigue, stress: e.stress },
    r = bo(e, t),
    i = r
      ? `身体不适，医疗费 HK$180，在家休养。`
      : `寒假在家调整状态。寒假活动（探亲、旅游、寻找实习）将在后续版本加入。`;
  return (
    (i += xo(e) + Co(e, t)),
    (e.fatigue = xi(e.fatigue)),
    (e.stress = xi(e.stress)),
    e.logs.push({
      day: t,
      half: null,
      date: I(t),
      title: r ? `寒假 · 生病休养` : `寒假`,
      text: i,
      money: e.money - n.money,
      fatigue: e.fatigue - n.fatigue,
      stress: e.stress - n.stress,
    }),
    e.cursor++,
    e.cursor === 21 && (e.phase = `summary`),
    e
  );
}
function Do(e) {
  if (
    e.phase !== `running` ||
    e.notice ||
    e.event ||
    e.blockedReason ||
    e.cursor >= wi(e.round)
  )
    return e;
  let t = R(e);
  if (Ci(t).kind === `winter`) return Eo(t);
  let n = t.cursor,
    { day: r, half: i } = Ei(t.round, n),
    a = ln(r, i),
    o = t.plan[n],
    s = { money: t.money, fatigue: t.fatigue, stress: t.stress },
    c = Ai(t, r, i),
    l = !!c && o === jn(c.id),
    u =
      i === `am`
        ? t.fatigue >= F.illness || t.stress >= F.illness
        : t.sickDay === r;
  if (!u) {
    let e = P(o),
      n =
        o === null
          ? `这个半日尚未安排，请选择一项活动。`
          : (Qa(t, o) ??
            (e && i === `am` && t.money < On.cost
              ? `余额不足：全天游玩需要 HK$${On.cost}，现在只有 HK$${t.money}。请改成其他安排。`
              : null));
    if (n) return (t.blockedReason = n), t;
  }
  let d = ``,
    f = ``;
  if (
    (u && i === `am`
      ? (bo(t, r),
        (d = `获批病假 · 在家休养`),
        (f = `医疗费 HK$180。今天上午和下午都在家休养，原定安排未执行。`))
      : u && ((d = `病假 · 继续休养`), (f = `下午继续休养，原定安排未执行。`)),
    a)
  ) {
    let e = a.course,
      r = Oi(t.round, n);
    if (a.kind === `class`) {
      let n = t.attendance[e];
      n.expected++, u ? n.excused++ : o === r && n.attended++;
    } else
      u
        ? (t.exams[a.id] = {
            status: `excused`,
            score: null,
            makeup: `pending`,
            legacy: !1,
          })
        : (t.exams[a.id] =
            o === r
              ? {
                  status: `taken`,
                  score: ea(
                    t.mastery[e],
                    t.abilities.research,
                    t.fatigue,
                    t.stress,
                  ),
                  makeup: null,
                  legacy: !1,
                }
              : { status: `missed`, score: 0, makeup: null, legacy: !1 });
  }
  if (c) {
    let e = t.exams[c.id];
    l
      ? ((e.makeup = `taken`),
        (e.score = ea(
          t.mastery[c.course],
          t.abilities.research,
          t.fatigue,
          t.stress,
        )))
      : ((e.makeup = `missed`), (e.score = 0));
  }
  if (l)
    (d = `补考 · ${na(c)}`),
      (f = `${u ? (i === `am` ? `身体不适，医疗费 HK$180；` : `身体不适；`) + `已安排的补考照常进行，当天其余时间休养。` : ``}按时参加补考，本场得分 ${t.exams[c.id].score}（与正式考试同一公式，不封顶压分）。`),
      (t.fatigue += Pn.fatigue),
      (t.stress += Pn.stress);
  else if (!u) {
    if (o?.startsWith(`class:`) || o?.startsWith(`exam:`)) {
      let e = o.slice(o.indexOf(`:`) + 1);
      if (
        ((d = o.startsWith(`exam:`) ? `考试 · ${$n(e)}` : $n(e)),
        (f = o.startsWith(`exam:`)
          ? `认真作答，交卷后松了一口气。本场得分 ${t.exams[a.id].score}。`
          : `听课、讨论、记下新的问题。`),
        (t.fatigue += Pn.fatigue),
        (t.stress += Pn.stress),
        o.startsWith(`class:`))
      ) {
        let n = Yn[e];
        Bi(t, n, Xn.class.ability),
          Vi(t, e, Xn.class.mastery),
          (f += ` ${Hi(n, Xn.class.ability)}，${$n(e)}掌握度 +${Xn.class.mastery}。`);
      }
    } else if (Ki(o)) {
      let e = gn(Ki(o));
      (d = `兼职 · ${e.name}`),
        (t.money += e.pay),
        (t.fatigue += e.fatigue),
        (t.stress += e.stress),
        t.jobShifts++,
        Bi(t, e.growth, e.growthPerShift),
        (f = `完成半日工作，工资 HK$${e.pay}。${Hi(e.growth, e.growthPerShift)}。`);
    } else if (rr(o)) {
      let e = rr(o);
      ({ title: d, text: f } = wo(t, e.kind, e.course, r, i));
    } else if (P(o)) {
      let e = Dn.find((e) => e.id === P(o));
      (d = `全天游玩 · ${e.name}`),
        i === `am`
          ? ((t.money -= On.cost),
            (f = `出发去${e.name}，今天的费用 HK$${On.cost} 已付。`))
          : ((t.fatigue -= On.fatigue),
            (t.stress -= On.stress),
            (f = `${e.text}尽兴而归：疲劳 −${On.fatigue}、压力 −${On.stress}。`));
    } else {
      let e = er(o);
      if (
        ((d = e.name),
        (f = {
          review: ``,
          rest: `慢慢吃饭、好好休息，这半天也有自己的价值。`,
          walk: `出门走走，公园和海旁都比书桌开阔。`,
        }[e.id]),
        (t.fatigue += e.fatigue),
        (t.stress += e.stress),
        (t.money += e.money),
        e.id === `rest`)
      ) {
        let e = Ri(t);
        (t.fatigue -= e.fatigue), (t.stress -= e.stress);
      }
      e.id === `walk` && go(t, r, i),
        e.id === `review` &&
          (Bi(t, `research`, Xn.review.ability),
          Vi(t, t.reviewFocus, Xn.review.mastery),
          (f = `复习${$n(t.reviewFocus)}，整理笔记。${Hi(`research`, Xn.review.ability)}，${$n(t.reviewFocus)}掌握度 +${Xn.review.mastery}。`));
    }
    a &&
      o !== Oi(t.round, n) &&
      (f +=
        a.kind === `class`
          ? ` 这节课记为无故缺勤。`
          : ` 这场考试记为无故缺考（0 分，按原权重计入）。`);
  }
  if (
    (c && !l && (f += ` 没有参加${na(c)}的补考，记为缺考（0 分）。`),
    a && a.kind !== `class` && u && (f += To(t, a, n)),
    a && (f += So(t, a, r)),
    i === `pm`)
  ) {
    f += xo(t) + Co(t, r);
    let e = [t.plan[n - 1], o].some(
      (e) =>
        e === `review` || e?.startsWith(`class:`) || e?.startsWith(`task:`),
    );
    t.event || vo(t, r, e);
  }
  return (
    (t.fatigue = xi(t.fatigue)),
    (t.stress = xi(t.stress)),
    t.logs.push({
      day: r,
      half: i,
      date: I(r),
      title: d,
      text: f,
      money: t.money - s.money,
      fatigue: t.fatigue - s.fatigue,
      stress: t.stress - s.stress,
    }),
    t.cursor++,
    t.cursor === 28 && (t.phase = `summary`),
    t
  );
}
function Oo(e) {
  if (e.phase !== `summary`) return e;
  if (e.round === Si) return { ...R(e), phase: `finished` };
  let t = R(e);
  Ci(t).kind !== `winter` && (t.lastPlan = [...t.plan]),
    t.history.push(...t.logs),
    t.round++,
    (t.plan = Ti(t.round));
  for (let e of dn) {
    if (t.exams[e.id]?.makeup !== `pending`) continue;
    let n = ki(e.id);
    n.round === t.round && (t.plan[n.index] = jn(e.id));
  }
  (t.cursor = 0), (t.phase = `planning`), (t.logs = []);
  let n = Ui(t);
  return (
    n.length &&
      !n.some((e) => e.id === t.reviewFocus) &&
      (t.reviewFocus = n[0].id),
    (t.purchased = bi()),
    t
  );
}
function ko(e) {
  if (e.phase !== `planning` && e.phase !== `running`) return null;
  let t = R(e);
  (t.plan = t.plan.map((e) => e ?? `rest`)),
    t.phase === `planning` && (t = so(t)),
    (t.notice = null),
    (t.event = null),
    (t.blockedReason = null);
  for (let e = 0; e < 64 && t.phase === `running`; e++)
    (t = Do(t)),
      t.notice && (t = co(t)),
      (t.event &&= null),
      t.blockedReason &&
        (P(t.plan[t.cursor]) &&
          t.cursor % 2 == 0 &&
          (t.plan[t.cursor + 1] = `rest`),
        (t.plan[t.cursor] = `rest`),
        (t.blockedReason = null));
  return { money: t.money, fatigue: t.fatigue, stress: t.stress };
}
function Ao(e, t) {
  let n = Vn.find((e) => e.id === t);
  if (![`planning`, `running`].includes(e.phase))
    return { state: e, error: `请先进入下一轮。` };
  if (e.money < n.price) return { state: e, error: `余额不足。` };
  if (
    e.purchased[t] >= n.stock ||
    (n.kind === `furniture` && e.inventory[t] > 0)
  )
    return { state: e, error: `本轮已售罄，或你已经拥有这件家具。` };
  let r = R(e);
  return (
    (r.money -= n.price),
    r.inventory[t]++,
    r.purchased[t]++,
    { state: r, error: null }
  );
}
function jo(e, t) {
  let n = Vn.find((e) => e.id === t);
  if (
    e.inventory[t] < 1 ||
    n.kind !== `item` ||
    ![`planning`, `running`].includes(e.phase)
  )
    return e;
  let r = R(e);
  return (
    r.inventory[t]--,
    t === `tea` && (r.stress = xi(r.stress - 8)),
    t === `bun` && (r.fatigue = xi(r.fatigue - 6)),
    r
  );
}
function Mo(e, t) {
  if (
    e.inventory[t] < 1 ||
    Vn.find((e) => e.id === t)?.kind !== `furniture` ||
    (!e.installed.includes(t) && zi(e) < 1)
  )
    return e;
  let n = R(e);
  return (
    (n.installed = n.installed.includes(t)
      ? n.installed.filter((e) => e !== t)
      : [...n.installed, t]),
    n
  );
}
var No = new Set(cn.filter((e) => e.kind !== `class`).map((e) => e.id)),
  Po = gi(0);
for (let e of cn) e.kind === `class` && Po[e.course]++;
var Fo = N.map((e) => e.id).join(`|`),
  Io = RegExp(
    `^((class|exam|task|project):(${Fo})|job:(${mn.map((e) => e.id).join(`|`)})|trip:(${Dn.map((e) => e.id).join(`|`)})|makeup:(${dn.map((e) => e.id).join(`|`)}))$`,
  );
function Lo(e, t, n) {
  let r = Mn(t);
  if (!r || !ta(r) || e.exams[r]?.status !== `excused`) return !1;
  let i = ki(r);
  return i.round === e.round && i.index === n;
}
function Ro(e, t) {
  if (
    !t ||
    ![`taken`, `missed`, `excused`].includes(t.status) ||
    typeof t.legacy != `boolean`
  )
    return !1;
  let n = (e) =>
    typeof e == `number` && Number.isFinite(e) && e >= 0 && e <= 100;
  return t.status === `taken`
    ? t.makeup === null && (t.legacy ? t.score === null : n(t.score))
    : t.status === `missed`
      ? t.makeup === null && t.score === 0 && !t.legacy
      : t.legacy
        ? t.makeup === null && t.score === null
        : t.makeup === `pending`
          ? t.score === null
          : t.makeup === `missed`
            ? t.score === 0
            : t.makeup === `taken` && n(t.score) && !!ta(e);
}
var zo = [
  `unreleased`,
  `active`,
  `complete`,
  `overdue`,
  `submitted`,
  `missing`,
];
function Bo(e, t, n) {
  if (
    !e ||
    !zo.includes(e.status) ||
    typeof e.late != `boolean` ||
    typeof e.legacy != `boolean`
  )
    return !1;
  let r = H(t, n),
    i = da(n, t);
  if (
    !Number.isFinite(e.progress) ||
    e.progress < 0 ||
    e.progress > r ||
    !Number.isInteger(e.polish) ||
    e.polish < 0 ||
    !Number.isInteger(e.halves) ||
    e.halves < 0 ||
    ![e.sumA, e.sumM].every((e) => Number.isFinite(e) && e >= 0) ||
    (e.halves === 0 && (e.sumA !== 0 || e.sumM !== 0))
  )
    return !1;
  let a = e.quality;
  if (a !== null && !(Number.isFinite(a) && a >= 0 && a <= zn.cap)) return !1;
  if (e.status === `unreleased`)
    return (
      e.released === null &&
      e.progress === 0 &&
      e.polish === 0 &&
      e.submittedDay === null &&
      !e.late &&
      a === null &&
      e.score === null &&
      e.halves === 0
    );
  if (!Number.isInteger(e.released) || e.released < 0 || e.released > i)
    return !1;
  if (e.status === `submitted`) {
    let t = e.submittedDay;
    if (!Number.isInteger(t) || t < e.released || e.progress !== r) return !1;
    if (e.legacy && a === null) {
      if (e.score !== null) return !1;
    } else if (a === null || e.score !== ia(Ir(e.late ? `late` : `on_time`, a)))
      return !1;
    return e.late ? t > i && t <= i + 14 && e.polish === 0 : t <= i;
  }
  return e.submittedDay !== null || e.late
    ? !1
    : e.status === `missing`
      ? e.score === 0 && e.progress < r && e.polish === 0 && a === null
      : e.score === null
        ? e.status === `complete`
          ? e.progress === r && a !== null && (e.halves > 0 || e.legacy)
          : e.progress < r && e.polish === 0 && a === null
        : !1;
}
function Vo(e, t, n) {
  return n.every((r, i) => {
    if (!P(r)) return !0;
    let a = i - (i % 2);
    return n[a] === r && n[a + 1] === r && !ji(e, t, a) && !ji(e, t, a + 1);
  });
}
function Ho(e) {
  if (
    !Number.isInteger(e.seed) ||
    !Array.isArray(e.npcs) ||
    !e.npcs.every(
      (e) =>
        e &&
        typeof e.id == `string` &&
        typeof e.name == `string` &&
        [`pool`, `fallback`, `legacy`].includes(e.kind) &&
        !!e.abilities &&
        qn.every(
          (t) =>
            Number.isFinite(e.abilities[t.id]) &&
            e.abilities[t.id] >= 0 &&
            e.abilities[t.id] <= 100,
        ),
    )
  )
    return !1;
  let t = e.npcs.map((e) => e.id);
  if (
    new Set(t).size !== t.length ||
    new Set(e.npcs.map((e) => e.name)).size !== t.length ||
    e.npcs.filter((e) => e.kind === `pool`).length !== 24 ||
    e.npcs.filter((e) => e.kind === `fallback`).length !== 2
  )
    return !1;
  let n = (e) =>
    typeof e == `number` && Number.isFinite(e) && e >= or.min && e <= or.max;
  if (
    !e.affection ||
    !t.every((t) => n(e.affection[t])) ||
    Object.keys(e.affection).length !== t.length ||
    !n(e.reputation) ||
    !Array.isArray(e.known) ||
    !e.known.every((e) => t.includes(e)) ||
    new Set(e.known).size !== e.known.length
  )
    return !1;
  let r = (t) => e.npcs.find((e) => e.id === t)?.kind;
  return N.every((t) => {
    let n = e.projects[t.id];
    return typeof n.locked != `boolean` ||
      typeof n.refreshed != `boolean` ||
      !Array.isArray(n.shown) ||
      n.shown.length > 4 ||
      !n.shown.every((t) => r(t) === `pool` && e.known.includes(t)) ||
      !n.invites ||
      typeof n.invites != `object` ||
      ![`pending`, `graded`, `legacy`].includes(n.relation) ||
      (n.status === `submitted` || n.status === `missing`
        ? n.relation === `pending` && e.event?.course !== t.id
        : n.relation !== `pending`) ||
      !Object.entries(n.invites).every(
        ([e, t]) =>
          r(e) === `pool` &&
          typeof t == `boolean` &&
          t === n.members.includes(e),
      ) ||
      !n.members.every(
        (e) => r(e) !== void 0 && (r(e) !== `pool` || n.invites[e] === !0),
      ) ||
      (n.progress > 0 || n.polish > 0) !== n.locked ||
      (n.locked && !ma(n))
      ? !1
      : n.status === `unreleased`
        ? n.members.length === 0 &&
          n.shown.length === 0 &&
          Object.keys(n.invites).length === 0 &&
          !n.refreshed
        : !0;
  });
}
function Uo(e) {
  if (typeof e.firstWalkDone != `boolean` || !Array.isArray(e.events))
    return !1;
  let t = (t) =>
    po(t.id) === `group`
      ? !!t.course &&
        N.some((e) => e.id === t.course) &&
        typeof t.npc == `string` &&
        e.npcs.some((e) => e.id === t.npc)
      : t.course === null && t.npc === null;
  if (
    !e.events.every(
      (n, r) =>
        n &&
        Br(n.id) &&
        Number.isInteger(n.round) &&
        n.round >= 0 &&
        n.round <= e.round &&
        Number.isInteger(n.day) &&
        t(n) &&
        (n.choice === null
          ? r === e.events.length - 1 && !!e.event
          : Number.isInteger(n.choice) &&
            n.choice >= 0 &&
            n.choice < Br(n.id).options.length),
    ) ||
    (e.events.some((e) => po(e.id) === `walk`) && !e.firstWalkDone) ||
    M.some((t) => {
      let n = e.events.filter((e) => e.round === t.index);
      return (
        n.length > Lr.perRoundCap ||
        Object.entries(Lr.categoryCap).some(
          ([e, t]) => n.filter((t) => po(t.id) === e).length > t,
        )
      );
    })
  )
    return !1;
  if (e.event === null) return !0;
  let n = e.events.at(-1),
    r = e.event;
  return (
    e.phase === `running` &&
    !!n &&
    n.id === r.id &&
    n.choice === null &&
    n.round === e.round &&
    n.day === r.day &&
    n.course === r.course &&
    n.npc === r.npc &&
    [`am`, `pm`].includes(r.half) &&
    (r.course === null || e.projects[r.course].members.includes(r.npc))
  );
}
var Wo = (e) => M.slice(0, e).reduce((e, t) => e + wi(t.index), 0);
function Go(e) {
  if (!e || typeof e != `object`) return !1;
  let t = e;
  if (
    t.version !== 9 ||
    !Number.isInteger(t.round) ||
    t.round < 0 ||
    t.round > Si ||
    ![`planning`, `running`, `summary`, `finished`].includes(t.phase)
  )
    return !1;
  let n = Hn.find((e) => e.id === t.economy),
    r = Un.find((e) => e.id === t.housing);
  if (
    !n ||
    !r ||
    t.budget !== n.budget ||
    t.rentPaid !== r.monthlyRent * 12 ||
    t.deposit !== r.monthlyRent
  )
    return !1;
  let i = wi(t.round);
  if (
    !Number.isInteger(t.cursor) ||
    t.cursor < 0 ||
    t.cursor > i ||
    (t.phase === `planning` && t.cursor !== 0) ||
    (t.phase === `running` && t.cursor >= i) ||
    ([`summary`, `finished`].includes(t.phase) && t.cursor !== i) ||
    (t.phase === `finished` && t.round !== Si)
  )
    return !1;
  let a = (e) =>
    Nn.every((t) => e.filter((e) => e === t.id).length <= t.cap) && qi(e) <= 6;
  if (
    V(t.initialAbilities) !== null ||
    !t.jobs ||
    !Array.isArray(t.jobs.hired) ||
    new Set(t.jobs.hired).size !== t.jobs.hired.length ||
    !t.jobs.hired.every((e) => gn(e))
  )
    return !1;
  let o = t.jobs.lastAttempt;
  if (
    (o !== null &&
      !(
        o &&
        Number.isInteger(o.round) &&
        o.round >= 0 &&
        o.round <= Si &&
        M[o.round].kind === `semester` &&
        gn(o.job) &&
        Number.isFinite(o.chance) &&
        o.chance > 0 &&
        o.chance <= 100 &&
        Number.isFinite(o.roll) &&
        o.roll >= 0 &&
        o.roll < 1 &&
        typeof o.success == `boolean` &&
        o.success === o.roll < o.chance / 100 &&
        (!o.success || t.jobs.hired.includes(o.job))
      )) ||
    !Array.isArray(t.plan) ||
    t.plan.length !== Ti(t.round).length ||
    !a(t.plan) ||
    !t.plan.every(
      (e, n) =>
        e === null ||
        Nn.some((t) => t.id === e) ||
        e === Oi(t.round, n) ||
        Lo(t, e, n) ||
        (Ki(e) !== null && t.jobs.hired.includes(Ki(e))) ||
        (rr(e) !== null && Io.test(e)) ||
        (P(e) !== null && Io.test(e)),
    ) ||
    !Vo(t, t.round, t.plan) ||
    t.plan
      .slice(0, Math.min(t.cursor, t.plan.length))
      .some((e) => e === null) ||
    (t.lastPlan !== null &&
      !(
        Array.isArray(t.lastPlan) &&
        t.lastPlan.length === 28 &&
        a(t.lastPlan) &&
        t.lastPlan.every(
          (e) =>
            e === null ||
            Nn.some((t) => t.id === e) ||
            (typeof e == `string` && Io.test(e)),
        )
      )) ||
    !Number.isFinite(t.money) ||
    Math.abs(t.money) > 1e7 ||
    ![t.fatigue, t.stress].every(
      (e) => Number.isFinite(e) && e >= 0 && e <= 100,
    ) ||
    !t.abilities ||
    !qn.every(
      (e) =>
        Number.isFinite(t.abilities[e.id]) &&
        t.abilities[e.id] >= t.initialAbilities[e.id] &&
        t.abilities[e.id] <= 100,
    ) ||
    !t.mastery ||
    !N.every(
      (e) =>
        Number.isFinite(t.mastery[e.id]) &&
        t.mastery[e.id] >= 0 &&
        t.mastery[e.id] <= 100,
    ) ||
    !N.some((e) => e.id === t.reviewFocus) ||
    (t.sickDay !== null &&
      !(Number.isInteger(t.sickDay) && t.sickDay >= 0 && t.sickDay < 245)) ||
    !t.attendance ||
    !t.tasks ||
    !t.projects ||
    !N.every((e) => {
      let n = t.attendance[e.id],
        r = t.projects[e.id];
      return (
        n &&
        [n.expected, n.attended, n.excused].every(
          (e) => Number.isInteger(e) && e >= 0,
        ) &&
        n.expected <= Po[e.id] &&
        n.attended + n.excused <= n.expected &&
        Bo(t.tasks[e.id], `individual`, e.id) &&
        Bo(r, `group`, e.id) &&
        Array.isArray(r.members) &&
        !!r.roles &&
        typeof r.roles == `object` &&
        Bn.every((e) => e.id in r.roles) &&
        (ma(r)
          ? r.status !== `unreleased`
          : r.members.length <= 1 &&
            Object.values(r.roles).every((e) => e === null) &&
            r.progress === 0 &&
            r.polish === 0)
      );
    }) ||
    !Ho(t) ||
    !Uo(t) ||
    !t.exams ||
    typeof t.exams != `object` ||
    !Object.entries(t.exams).every(([e, t]) => No.has(e) && Ro(e, t)) ||
    !Number.isInteger(t.jobShifts) ||
    t.jobShifts < 0 ||
    (t.destination !== null && !In.some((e) => e.id === t.destination)) ||
    (t.ending !== null &&
      !(
        t.phase === `finished` &&
        t.ending &&
        t.ending.destination === t.destination &&
        [`graduated`, `delayed`, `incomplete`].includes(t.ending.graduation) &&
        typeof t.ending.scholarship == `boolean`
      )) ||
    (t.notice !== null &&
      (!t.notice ||
        !N.some((e) => e.id === t.notice.course) ||
        ![`individual`, `group`, `makeup`].includes(t.notice.kind) ||
        (t.notice.kind === `makeup`
          ? !dn.some(
              (e) =>
                e.course === t.notice.course &&
                t.exams[e.id]?.status === `excused`,
            )
          : ua(t, t.notice.kind, t.notice.course).status === `unreleased`))) ||
    (t.blockedReason !== null &&
      (typeof t.blockedReason != `string` ||
        t.blockedReason.length > 200 ||
        t.phase !== `running`)) ||
    !t.inventory ||
    !t.purchased ||
    !Array.isArray(t.installed) ||
    !Vn.every(
      (e) =>
        Number.isInteger(t.inventory[e.id]) &&
        t.inventory[e.id] >= 0 &&
        t.inventory[e.id] < 100 &&
        Number.isInteger(t.purchased[e.id]) &&
        t.purchased[e.id] >= 0 &&
        t.purchased[e.id] <= e.stock &&
        (e.kind !== `furniture` || t.inventory[e.id] <= 1),
    ) ||
    new Set(t.installed).size !== t.installed.length ||
    t.installed.length > r.slots ||
    !t.installed.every(
      (e) =>
        t.inventory[e] === 1 &&
        Vn.some((t) => t.id === e && t.kind === `furniture`),
    )
  )
    return !1;
  let s = (e) =>
    !!e &&
    Number.isInteger(e.day) &&
    e.day >= 0 &&
    e.day < 245 &&
    (e.half === null || e.half === `am` || e.half === `pm`) &&
    [`title`, `text`, `date`].every((t) => typeof e[t] == `string`) &&
    [e.money, e.fatigue, e.stress].every(Number.isFinite);
  return (
    typeof t.name == `string` &&
    t.name.length > 0 &&
    t.name.length <= 20 &&
    qr(t.appearance) &&
    Array.isArray(t.logs) &&
    t.logs.length === t.cursor &&
    t.logs.every(s) &&
    Array.isArray(t.history) &&
    t.history.length === Wo(t.round) &&
    t.history.every(s) &&
    !!t.roundStart &&
    [t.roundStart.money, t.roundStart.fatigue, t.roundStart.stress].every(
      Number.isFinite,
    )
  );
}
var Xo = `assets/lite-media`,
  Zo = `用户提供并要求接入（2026-10-20 交付包 HK_Lite_UI_B2_Assets_v1.0）；原作者或平台许可细节未附，不推断为 CC0`,
  Qo = `user-delivery-v1.0`,
  $o = `用户本次提供并要求接入（2026-10-21 交接包 HK_Lite_UI_C_Start_v1.0）；原作者或平台许可细节未附，不推断为 CC0`,
  es = (e, t, n, r, i) => ({
    asset: e,
    src: `${Xo}/${t}`,
    alt: n,
    source: Zo,
    version: Qo,
    provided: !0,
    scope: r,
    brief: i,
  }),
  ts = (e, t) => ({
    asset: null,
    src: `${Xo}/cg/${e}`,
    alt: t,
    source: `待制作：尚未排期`,
    version: `—`,
    provided: !1,
    scope: `all`,
    brief: `暂不排期，事件以文字显示。`,
  }),
  ns = es(
    `CG03`,
    `cg/group_discussion_female_young.webp`,
    `阳光下的讨论桌，青年女主和两位背景同学一起讨论`,
    `female_young`,
    `四个小组事件共用；画面是当前青年女主，背景同学不是本局具体的 NPC。其他角色的小组事件继续用文字。`,
  ),
  rs = {
    event_red_bulbul: es(
      `CG01`,
      `cg/red_bulbul.webp`,
      `香港公园枝头的红耳鹎`,
      `all`,
      `公园红耳鹎事件。`,
    ),
    event_daily_small_win: es(
      `CG02`,
      `cg/daily_small_win.webp`,
      `窗边书桌上的旧笔记与台灯`,
      `all`,
      `看懂旧笔记的日常事件。`,
    ),
    event_group_outline: ns,
    event_group_delay: ns,
    event_group_credit: ns,
    event_group_rehearsal: ns,
    event_walk_sky: es(
      `CG04`,
      `cg/walk_sky.webp`,
      `海边被落日染成橘粉色的天空`,
      `all`,
      `散步遇到海边落日的随机事件；不等于全天维港旅行。`,
    ),
    event_walk_shower: ts(`walk_shower.webp`, `街角突然落下的阵雨`),
    event_walk_music: ts(`walk_music.webp`, `广场边的弹唱`),
    event_daily_call: ts(`daily_call.webp`, `睡前亮起的手机`),
  },
  is = () => globalThis.__HK_INLINE_ASSETS__,
  as = (e, t) => is()?.[e] ?? `${t.endsWith(`/`) ? t : `${t}/`}${e}`,
  os = (e) => !!e && (e.provided || !!is()?.[e.src]),
  ss = (e) => os(e ? rs[e] : void 0);
function cs(e, t) {
  let n = e ? rs[e] : void 0;
  return !!n && (n.scope === `all` || n.scope === t);
}
function ls(e, t = `/`, n) {
  return !ss(e) || (n !== void 0 && !cs(e, n)) ? null : as(rs[e].src, t);
}
var us = (e, t, n) => ({
    asset: e,
    src: `${Xo}/${t}`,
    alt: n,
    source: Zo,
    version: Qo,
    provided: !0,
  }),
  ds = {
    victoria_harbour: {
      scenery: us(
        `TR01`,
        `travel/victoria_harbour_scenery.webp`,
        `黄昏的维多利亚港，渡轮驶过海面`,
      ),
      line: `把海港的晚霞收进今天。`,
      selfie: {
        ...us(
          `SF01`,
          `selfies/victoria_harbour_female_young.webp`,
          `维港海旁，青年女主比着手势自拍`,
        ),
        body: `female_young`,
      },
    },
    maclehose: {
      scenery: us(
        `TR02`,
        `travel/maclehose_scenery.webp`,
        `麦理浩径的山脊小路与远处的海湾`,
      ),
      line: `把山和海一起装进今天。`,
      selfie: {
        ...us(
          `SF02`,
          `selfies/maclehose_female_young.webp`,
          `麦理浩径山路上，戴遮阳帽的青年女主自拍`,
        ),
        body: `female_young`,
      },
    },
  },
  fs = (e) => os(ds[e]?.scenery),
  ps = (e) => {
    let t = ds[e]?.selfie;
    return t ? `${t.asset}@${t.version}` : null;
  };
function ms(e, t, n = `/`) {
  let r = ds[e],
    i = r ? (t === `scenery` ? r.scenery : r.selfie) : null;
  return i && os(i) ? as(i.src, n) : null;
}
var hs = `全天游玩 · `,
  gs = (e) => M.find((t) => e >= t.startDay && e <= t.endDay)?.index ?? -1,
  _s = (e, t, n) => `${e}:${t}:${n}`;
function vs(e) {
  return e.half !== `pm` || !e.title.startsWith(hs)
    ? null
    : (Dn.find((t) => e.title === `${hs}${t.name}`)?.id ?? null);
}
function ys(e) {
  let t = [];
  for (let n of [...e.history, ...e.logs]) {
    let e = vs(n);
    if (!e) continue;
    let r = gs(n.day);
    t.push({ key: _s(r, n.day, e), loc: e, round: r, day: n.day });
  }
  return t;
}
function bs(e, t, n) {
  if (t.logs.length <= e.logs.length && t.round === e.round)
    return { meta: n, recorded: [] };
  let r = t.round === e.round ? t.logs.slice(e.logs.length) : t.logs,
    i = n.trips ?? [],
    a = [];
  for (let e of r) {
    let n = vs(e);
    if (!n || !fs(n)) continue;
    let r = _s(t.round, e.day, n);
    if ([...i, ...a].some((e) => e.key === r)) continue;
    let o = t.appearance.bodyId,
      s = ds[n]?.selfie?.body === o ? ps(n) : null,
      c = [...i, ...a].some(
        (e) => e.loc === n && e.round === t.round && e.review !== `silent`,
      );
    a.push({
      key: r,
      loc: n,
      round: t.round,
      day: e.day,
      body: o,
      selfie: s,
      review: c ? `silent` : `pending`,
    });
  }
  if (!a.length) return { meta: n, recorded: a };
  let o = a
    .filter((e) => e.review === `silent`)
    .map((e) => ({
      round: e.round,
      day: e.day,
      text: `${Dn.find((t) => t.id === e.loc).name}的旅行已记录到手账。`,
    }));
  return {
    meta: { ...n, trips: [...i, ...a], digest: [...n.digest, ...o] },
    recorded: a,
  };
}
var xs = (e) => e.trips?.find((e) => e.review === `pending`) ?? null,
  Ss = (e, t) =>
    e.trips?.some((e) => e.key === t && e.review === `pending`)
      ? {
          ...e,
          trips: e.trips.map((e) =>
            e.key === t ? { ...e, review: `shown` } : e,
          ),
        }
      : e;
function Cs(e, t) {
  return ys(e).map((e) => {
    let n = t.trips?.find((t) => t.key === e.key),
      r = Dn.find((t) => t.id === e.loc);
    return {
      key: e.key,
      loc: e.loc,
      name: r.name,
      round: e.round,
      day: e.day,
      date: I(e.day),
      text: ds[e.loc]?.line ?? r.text,
      scenery: fs(e.loc),
      selfie:
        !!n?.selfie &&
        n.selfie === ps(e.loc) &&
        ds[e.loc]?.selfie?.body === n.body,
      backfilled: fs(e.loc) && !n,
    };
  });
}
var ws = (e) => Dn.some((t) => t.id === e);
function Ts(e) {
  return (
    e === void 0 ||
    (Array.isArray(e) &&
      e.length <= 200 &&
      e.every(
        (e) =>
          e &&
          typeof e == `object` &&
          typeof e.key == `string` &&
          e.key.length < 60 &&
          ws(e.loc) &&
          Number.isInteger(e.round) &&
          e.round >= 0 &&
          e.round < M.length &&
          Number.isInteger(e.day) &&
          e.key === _s(e.round, e.day, e.loc) &&
          Vr.includes(e.body) &&
          (e.selfie === null ||
            (typeof e.selfie == `string` && e.selfie.length < 60)) &&
          [`pending`, `shown`, `silent`].includes(e.review),
      ))
  );
}
var Es = (e) =>
  e.event ? `event:${e.round}:${e.event.day}:${e.event.id}` : null;
function Ds(e, t) {
  let n = Es(e);
  return !n || t.looks?.some((e) => e.key === n)
    ? t
    : {
        ...t,
        looks: [
          ...(t.looks ?? []),
          { key: n, body: e.appearance.bodyId },
        ].slice(-200),
      };
}
var Os = (e, t) => e.looks?.find((e) => e.key === t)?.body ?? null;
function ks(e) {
  return (
    e === void 0 ||
    (Array.isArray(e) &&
      e.length <= 200 &&
      e.every(
        (e) =>
          e &&
          typeof e == `object` &&
          typeof e.key == `string` &&
          e.key.length < 120 &&
          Vr.includes(e.body),
      ))
  );
}
var As = [
    {
      id: `academic`,
      name: `学业成长`,
      hint: `作业提早完成，多一些复习；休息照样安排。`,
      detail: `已发布、能开展的作业尽量提早完成，每轮最多 4 次复习、1 次散步，其余休息。`,
    },
    {
      id: `life`,
      name: `香港生活`,
      hint: `必要作业按进度完成，其余时间多出门走走。`,
      detail: `作业按截止进度分配（保证节奏、不提早赶完），每轮 3 次散步；余额足够时安排 1 天全天游玩（HK$300），其余休息。`,
    },
    {
      id: `work`,
      name: `工作体验`,
      hint: `已录用的兼职排进日程，作业按进度完成。`,
      detail: `已录用岗位每轮最多 6 个兼职半日（原规则上限，按实际执行发薪）；作业按截止进度分配，每轮 1 次散步，其余休息。还没录用时先给出一张可申请的岗位卡。`,
    },
  ],
  js = {
    id: `balanced`,
    name: `平衡生活（旧存档）`,
    hint: `旧存档的安排，延续到本阶段结束。`,
    detail: `每轮最多 8 个作业半日，2 次复习，其余休息；下个阶段开始时请从三种倾向中选一个。`,
  },
  Ms = (e) => (e === `balanced` ? js : As.find((t) => t.id === e)),
  Ns = (e) => Ms(e).name;
function Ps(e) {
  let t = M[e];
  return !t || t.kind !== `semester`
    ? -1
    : (t.semester === 1 ? 0 : 100) + Math.floor((t.roundInSemester - 1) / 2);
}
var Fs = (e) => {
    let t = M[e];
    return t?.kind === `semester` && (t.roundInSemester - 1) % 2 == 0;
  },
  Is = (e) => {
    let t = Ps(e);
    return t < 0 ? `` : `第 ${(t % 100) + 1} 阶段`;
  };
function Ls() {
  return {
    v: 2,
    focus: `life`,
    focusPhase: -1,
    round: -1,
    free: [],
    auto: [],
    customized: !1,
    seen: [],
    teamPrompted: [],
    defaultTeams: [],
    digest: [],
    conflicts: [],
    monologues: [],
    jobCardDismissed: -1,
    relaxed: [],
    health: null,
    conflict: null,
    workQuota: { round: -1, items: {} },
  };
}
var Rs = (e) => Number.isInteger(e) && e >= 0 && e < 28,
  zs = (e) => N.some((t) => t.id === e),
  Bs = (e) => e === `balanced` || As.some((t) => t.id === e);
function Vs(e) {
  if (!e || typeof e != `object`) return !1;
  let t = e;
  return [`study`, `balanced`, `city`].includes(t.focus) && Hs(t);
}
function Hs(e) {
  return (
    Number.isInteger(e.round) &&
    e.round >= -1 &&
    e.round < M.length &&
    Array.isArray(e.free) &&
    e.free.length <= 2 &&
    e.free.every(Rs) &&
    Array.isArray(e.auto) &&
    e.auto.every(Rs) &&
    new Set(e.auto).size === e.auto.length &&
    typeof e.customized == `boolean` &&
    Array.isArray(e.seen) &&
    e.seen.every((e) => typeof e == `string` && e.length < 120) &&
    Array.isArray(e.teamPrompted) &&
    e.teamPrompted.every(zs) &&
    Array.isArray(e.defaultTeams) &&
    e.defaultTeams.every(zs) &&
    Array.isArray(e.digest) &&
    e.digest.length < 1e3 &&
    e.digest.every(
      (e) =>
        e &&
        Number.isInteger(e.round) &&
        Number.isInteger(e.day) &&
        typeof e.text == `string`,
    )
  );
}
function Us(e) {
  let t =
    e.focus === `study` ? `academic` : e.focus === `city` ? `life` : `balanced`;
  return {
    ...structuredClone(e),
    v: 2,
    focus: t,
    focusPhase: Ps(Math.max(0, e.round)),
    conflicts: [],
    monologues: [],
    jobCardDismissed: -1,
    relaxed: [],
    health: null,
    conflict: null,
    workQuota: { round: -1, items: {} },
  };
}
function Ws(e) {
  if (!e || typeof e != `object`) return !1;
  let t = e;
  return (
    t.v === 2 &&
    Bs(t.focus) &&
    Number.isInteger(t.focusPhase) &&
    Hs(t) &&
    Array.isArray(t.conflicts) &&
    t.conflicts.length < 200 &&
    t.conflicts.every(
      (e) =>
        e &&
        typeof e.key == `string` &&
        (e.choice === `rest` || e.choice === `push`),
    ) &&
    Array.isArray(t.monologues) &&
    t.monologues.every(
      (e) => e && Number.isInteger(e.round) && typeof e.key == `string`,
    ) &&
    Number.isInteger(t.jobCardDismissed) &&
    Array.isArray(t.relaxed) &&
    t.relaxed.every(Rs) &&
    (t.health === null ||
      (typeof t.health == `object` &&
        typeof t.health.triggered == `boolean` &&
        typeof t.health.resolved == `boolean` &&
        Number.isInteger(t.health.rested))) &&
    (t.conflict === null ||
      (typeof t.conflict == `object` &&
        typeof t.conflict.key == `string` &&
        Array.isArray(t.conflict.items))) &&
    Ts(t.trips) &&
    ks(t.looks) &&
    (t.dayCheck === void 0 ||
      t.dayCheck === null ||
      (typeof t.dayCheck == `string` && t.dayCheck.length < 20)) &&
    !!t.workQuota &&
    Number.isInteger(t.workQuota.round) &&
    !!t.workQuota.items &&
    typeof t.workQuota.items == `object` &&
    Object.values(t.workQuota.items).every(
      (e) => Number.isInteger(e) && e >= 0 && e <= 60,
    )
  );
}
var Gs = `hk-master-life-lite`,
  Ks = `hk-master-life-lite-save-v1`,
  qs = `hk-master-life-lite-save-v1-unreadable`,
  Js = `hk-master-life-lite-save-v1-before-job`,
  Ys = `hk-master-life-lite-save-v1-before-job-cap-repair`,
  Xs = `hk-master-life-lite-save-v1-schema1-backup`,
  Zs = `HK-Master-Life-LITE-save`;
function Qs(e, t) {
  return { productId: Gs, schemaVersion: 2, state: e, lite: t };
}
function $s(e, t) {
  if (!e || !Array.isArray(e.plan) || qi(e.plan) <= 6) return null;
  let n = [...e.plan],
    r = z(e),
    i = new Set(t?.auto ?? []),
    a = n
      .map((e, t) => (t >= r && Ki(e) !== null ? t : -1))
      .filter((e) => e >= 0),
    o = [
      ...a.filter((e) => i.has(e)).reverse(),
      ...a.filter((e) => !i.has(e)).reverse(),
    ];
  for (let e of o) {
    if (qi(n) <= 6) break;
    n[e] = `rest`;
  }
  let s = { ...e, plan: n };
  return Go(s) ? s : null;
}
function ec(e) {
  if (!e || typeof e != `object`)
    return { save: null, error: `文件不是存档。` };
  let t = e;
  if (t.productId !== `hk-master-life-lite`)
    return {
      save: null,
      error:
        typeof t.version == `number` && `plan` in t
          ? `这是原版《港硕生活模拟器》的存档，轻松版不能读取。原版存档不会被改动，请用原版试玩包继续。`
          : `这不是轻松版的存档文件（缺少轻松版标识）。`,
    };
  let n = t.schemaVersion;
  if (n !== 2 && n !== 1)
    return {
      save: null,
      error: `存档版本 ${String(n)} 与当前轻松版（2）不一致，暂不能读取。`,
    };
  let r = t.state,
    i = !1,
    a;
  if (!Go(r)) {
    let e = $s(r, t.lite ?? null);
    if (!e)
      return { save: null, error: `存档里的游戏进度已损坏或不完整，未读取。` };
    (a = e.plan
      .map((e, t) => (e === r.plan[t] ? -1 : t))
      .filter((e) => e >= 0)),
      (r = e),
      (i = !0);
  }
  return n === 1
    ? Vs(t.lite)
      ? {
          save: Qs(r, Us(t.lite)),
          error: null,
          upgraded: !0,
          repaired: i,
          repairedSlots: a,
        }
      : { save: null, error: `存档里的轻松版记录已损坏，未读取。` }
    : Ws(t.lite)
      ? { save: Qs(r, t.lite), error: null, repaired: i, repairedSlots: a }
      : { save: null, error: `存档里的轻松版记录已损坏，未读取。` };
}
function tc(e) {
  try {
    return JSON.parse(e);
  } catch {
    return null;
  }
}
function nc(e) {
  let t = e.getItem(Ks);
  if (t === null) return { save: null, unreadable: !1 };
  let n = ec(tc(t));
  return n.save
    ? (n.upgraded &&
        e.getItem(`hk-master-life-lite-save-v1-schema1-backup`) === null &&
        e.setItem(Xs, t),
      n.repaired &&
        e.getItem(`hk-master-life-lite-save-v1-before-job-cap-repair`) ===
          null &&
        e.setItem(Ys, t),
      {
        save: n.save,
        unreadable: !1,
        upgraded: !!n.upgraded,
        repaired: !!n.repaired,
        repairedSlots: n.repairedSlots,
      })
    : (e.getItem(`hk-master-life-lite-save-v1-unreadable`) === null &&
        e.setItem(qs, t),
      { save: null, unreadable: !0 });
}
var rc = (e) => !!e.event && (e.phase === `summary` || e.phase === `finished`);
function ic(e, t, n) {
  return !Go(t) || !Ws(n) ? !1 : (e.setItem(Ks, JSON.stringify(Qs(t, n))), !0);
}
function ac(e, t, n) {
  e.setItem(Js, JSON.stringify(Qs(t, n)));
}
function oc(e) {
  let t = e.getItem(Js);
  return t === null ? null : ec(tc(t)).save;
}
function sc(e, t = new Date()) {
  return `${Zs}_round${String(e.round + 1).padStart(2, `0`)}_${t.toISOString().slice(0, 10)}.json`;
}
function cc(e, t) {
  let n = e.plan[t];
  if (n?.startsWith(`exam:`)) {
    let n = Di(e.round, t);
    return n && n.kind !== `class` ? { course: n.course, kind: n.kind } : null;
  }
  let r = Mn(n),
    i = r ? dn.find((e) => e.id === r) : null;
  return i ? { course: i.course, kind: `makeup` } : null;
}
function lc(e) {
  let t = { days: [], exams: [] };
  if (e.phase !== `planning` && e.phase !== `running`) return t;
  let n = { ...structuredClone({ ...e, logs: [], history: [] }) };
  (n.plan = n.plan.map((e) => e ?? `rest`)),
    n.phase === `planning` && (n = so(n)),
    (n.notice = null),
    (n.event = null),
    (n.blockedReason = null);
  let r = n.plan.length;
  for (let i = 0; i < 80 && n.phase === `running` && n.cursor < r; i++) {
    let r = n.cursor,
      i = cc(n, r);
    if (
      (i &&
        t.exams.push({
          ...i,
          slot: r,
          day: Ei(n.round, r).day,
          fatigue: n.fatigue,
          stress: n.stress,
        }),
      (n = Do(n)),
      n.notice && (n = co(n)),
      (n.event &&= null),
      n.blockedReason)
    ) {
      P(n.plan[n.cursor]) &&
        n.cursor % 2 == 0 &&
        (n.plan[n.cursor + 1] = `rest`),
        (n.plan[n.cursor] = `rest`),
        (n.blockedReason = null);
      continue;
    }
    n.cursor > r &&
      r % 2 == 1 &&
      t.days.push({
        day: Ei(e.round, r).day,
        slot: r,
        fatigue: n.fatigue,
        stress: n.stress,
      });
  }
  return t;
}
var uc = {
    academic: { extraWork: 1 / 0, review: 4, walks: 1, trip: !1, jobs: 0 },
    life: { extraWork: 0, review: 0, walks: 3, trip: !0, jobs: 0 },
    work: { extraWork: 0, review: 0, walks: 1, trip: !1, jobs: 6 },
    balanced: { extraWork: 8, review: 2, walks: 0, trip: !1, jobs: 0 },
  },
  dc = {
    triggerEnergy: 30,
    triggerStress: 70,
    targetEnergy: 40,
    targetStress: 60,
  },
  fc = (e) => structuredClone(e),
  pc = (e) => 100 - e,
  mc = (e) => !!e && (e.startsWith(`task:`) || e.startsWith(`project:`));
function hc(e) {
  let t = [];
  for (let n = z(e); n < e.plan.length; n++) ji(e, e.round, n) || t.push(n);
  return t;
}
function gc(e) {
  let t = [];
  for (let n of N)
    for (let r of [`individual`, `group`]) {
      if (Za(e, r, n.id)) continue;
      let i = ua(e, r, n.id);
      if (i.status !== `active` && i.status !== `overdue`) continue;
      let a = _a(e, r, n.id);
      if (a <= 0) continue;
      let o = Math.ceil((H(r, n.id) - i.progress) / a);
      t.push({
        item: r === `individual` ? tr(n.id) : nr(n.id),
        kind: r,
        course: n.id,
        due: i.status === `overdue` ? fa(n.id, r) : da(n.id, r),
        need: o,
      });
    }
  return t.sort(
    (e, t) =>
      e.due - t.due ||
      N.findIndex((t) => t.id === e.course) -
        N.findIndex((e) => e.id === t.course),
  );
}
var _c = 10 / 7;
function vc(e, t) {
  let n = M[e.round].endDay;
  if (t.due <= n) return t.need;
  let r = Math.max(1, hc(e).length),
    i = (t.due - n) * _c;
  return Math.min(t.need, Math.ceil((t.need * 1.1 * r) / (r + i)));
}
var yc = { work: 0, walks: 0, review: 0, trip: 0, jobs: 0 };
function bc(e) {
  let t = e.plan.slice(0, z(e));
  return {
    work: t.filter(mc).length,
    walks: t.filter((e) => e === `walk`).length,
    review: t.filter((e) => e === `review`).length,
    trip: +!!t.some((e) => P(e)),
    jobs: t.filter((e) => Ki(e)).length,
  };
}
function xc(e, t) {
  let n = t.workQuota.round === e.round ? { ...t.workQuota.items } : {};
  for (let t of gc(e)) t.item in n || (n[t.item] = vc(e, t));
  return { round: e.round, items: n };
}
var Sc = (e, t) => e.plan.slice(0, z(e)).filter((e) => e === t).length;
function Cc(e, t, n, r, i, a) {
  let o = uc[r],
    s = [...t],
    c = z(e),
    l = M[e.round].endDay,
    u = [...n].sort((e, t) => e - t),
    d = (e) => {
      let t = u.find(e);
      return t === void 0 ? null : ((u = u.filter((e) => e !== t)), t);
    },
    f = (e) => s.filter((t, r) => r >= c && !n.includes(r) && t === e).length,
    p = (t) => Ei(e.round, t).day,
    m = (e) => Xt(p(e)) >= 5,
    h = gc(e),
    g = new Map();
  for (let t of h.filter(
    (t) => t.kind === `group` && !e.projects[t.course].locked,
  )) {
    if (f(t.item) > 0) continue;
    let e = d(() => !0);
    if (e === null) break;
    (s[e] = t.item), g.set(t.item, 1);
  }
  let _ = h.map((t) => ({
      w: t,
      n: Math.max(
        0,
        (t.due <= l ? t.need : (a[t.item] ?? vc(e, t)) - Sc(e, t.item)) -
          f(t.item) -
          (g.get(t.item) ?? 0),
      ),
    })),
    v = Math.min(
      u.length,
      _.reduce((e, t) => e + t.n, 0),
    ),
    y = Array.from(
      { length: v },
      (e, t) => u[Math.floor(((t + 0.5) * u.length) / v)],
    );
  u = u.filter((e) => !y.includes(e));
  let b = 0,
    x = i.work;
  for (let { w: e, n: t } of _)
    for (let n = 0; n < t && b < y.length; n++, b++, x++) s[y[b]] = e.item;
  for (let e of h) {
    let t =
      e.need -
      f(e.item) -
      s.filter((t, r) => n.includes(r) && t === e.item).length;
    for (; t > 0 && x < o.extraWork; ) {
      let n = d(() => !0);
      if (n === null) break;
      (s[n] = e.item), t--, x++;
    }
  }
  if (o.jobs > 0 && e.jobs.hired.length) {
    let t = [...e.jobs.hired].sort(
        (e, t) =>
          mn.find((e) => e.id === t).pay - mn.find((t) => t.id === e).pay,
      )[0],
      r = s.filter((e, t) => t >= c && !n.includes(t) && Ki(e)).length;
    for (let e = i.jobs + r; e < Math.min(o.jobs, 6); e++) {
      let e = d((e) => m(e)) ?? d(() => !0);
      if (e === null) break;
      s[e] = Gi(t);
    }
  }
  if (
    o.trip &&
    i.trip === 0 &&
    !s.some((e, t) => t >= c && !n.includes(t) && P(e)) &&
    e.money >= On.cost
  ) {
    let t = [
      ...u.filter((e) => e % 2 == 0 && u.includes(e + 1) && m(e)),
      ...u.filter((e) => e % 2 == 0 && u.includes(e + 1)),
    ][0];
    if (t !== void 0) {
      let n = Dn[(e.round + Math.floor(t / 2)) % Dn.length].id;
      (s[t] = kn(n)),
        (s[t + 1] = kn(n)),
        (u = u.filter((e) => e !== t && e !== t + 1));
    }
  }
  for (let e = i.walks + f(`walk`); e < o.walks; e++) {
    let e = d((e) => m(e) && e % 2 == 1) ?? d((e) => e % 2 == 1) ?? d(() => !0);
    e !== null && (s[e] = `walk`);
  }
  for (let e = i.review + f(`review`); e < o.review; e++) {
    let e = d((e) => !m(e) && e % 2 == 0) ?? d((e) => !m(e)) ?? d(() => !0);
    e !== null && (s[e] = `review`);
  }
  for (let e of u) s[e] = `rest`;
  return s;
}
var wc = (e, t) => `${e}:${[...new Set(t)].sort().join(`,`)}`;
function Tc(e, t, n, r) {
  let i = new Set(),
    a = M[e.round].endDay;
  for (let o of gc(e)) {
    let s = n.filter((e) => t[e] === o.item).sort((e, t) => e - t),
      c = o.due <= a ? o.need : (r[o.item] ?? vc(e, o)) - Sc(e, o.item),
      l = Math.max(
        0,
        c -
          t.filter((t, r) => r >= z(e) && !n.includes(r) && t === o.item)
            .length,
      );
    s.slice(l).forEach((e) => i.add(e));
  }
  return i;
}
function Ec(e, t, n, r) {
  if (Ki(t)) return 0;
  if (t === `review`) return 1;
  if (mc(t) && r.has(n)) return 2;
  if (mc(t)) {
    let n = gc(e).find((e) => e.item === t);
    return n && n.due - Mi(e) > 7 ? 3 : -1;
  }
  return t === `walk` ? 4 : P(t) ? 5 : -1;
}
function Dc(e, t, n, r) {
  let i = [...n],
    a = {
      triggered: !1,
      resolved: !0,
      replaced: [],
      risk: null,
      conflict: null,
      restedNecessary: [],
    },
    o = Mi(e);
  for (let n = 0; n < 40; n++) {
    let n = lc({ ...e, plan: i });
    if (!a.triggered) {
      if (
        !n.days.find(
          (e) =>
            pc(e.fatigue) < dc.triggerEnergy || e.stress > dc.triggerStress,
        )
      )
        return { plan: i, info: a };
      a.triggered = !0;
    }
    let s = n.days.filter(
      (e) => pc(e.fatigue) < dc.targetEnergy || e.stress > dc.targetStress,
    );
    if (!s.length)
      return (a.risk = null), (a.resolved = !0), { plan: i, info: a };
    (a.risk = { day: s[0].day, energy: pc(s[0].fatigue), stress: s[0].stress }),
      (a.resolved = !1);
    let c = Tc(e, i, r, t.workQuota.items),
      l;
    for (let t of s)
      if (
        ((l = r
          .filter(
            (n) => n <= t.slot && i[n] !== `rest` && Ec(e, i[n], n, c) >= 0,
          )
          .sort((t, n) => Ec(e, i[t], t, c) - Ec(e, i[n], n, c) || n - t)[0]),
        l !== void 0)
      )
        break;
    if (l !== void 0) {
      if (P(i[l])) {
        let e = l - (l % 2);
        (i[e] = `rest`), (i[e + 1] = `rest`), a.replaced.push(e, e + 1);
      } else (i[l] = `rest`), a.replaced.push(l);
      continue;
    }
    let u = s.at(-1),
      d = r
        .filter((e) => e <= u.slot && mc(i[e]))
        .filter((t) => {
          let n = gc(e).find((e) => e.item === i[t]);
          return n && n.due - o <= 7;
        });
    if (!d.length) return { plan: i, info: a };
    let f = gc(e)
        .filter((e) => d.some((t) => i[t] === e.item))
        .map((e) => ({
          item: e.item,
          course: e.course,
          kind: e.kind,
          due: e.due,
          halves: d.filter((t) => i[t] === e.item),
        })),
      p = wc(
        e.round,
        f.map((e) => e.item),
      ),
      m = t.conflicts.find((e) => e.key === p)?.choice;
    if (m === `rest`) {
      let e = d.at(-1);
      (i[e] = `rest`), a.replaced.push(e), a.restedNecessary.push(e);
      continue;
    }
    return (
      m === `push`
        ? (a.pushed = p)
        : (a.conflict = {
            key: p,
            items: f,
            day: s[0].day,
            energy: pc(s[0].fatigue),
            stress: s[0].stress,
          }),
      { plan: i, info: a }
    );
  }
  return { plan: i, info: a };
}
function Oc(e, t) {
  let n = {
    triggered: t.triggered,
    resolved: t.resolved,
    rested: t.replaced.length,
    risk: t.risk,
  };
  return {
    ...e,
    health: n,
    conflict: t.conflict
      ? {
          ...t.conflict,
          items: t.conflict.items.map((e) => ({ ...e, item: e.item })),
        }
      : null,
  };
}
function kc(e, t, n, r, i, a) {
  let o = xc(e, t),
    s = Cc(e, n, r, i, a, o.items);
  return { ...Dc({ ...e, plan: s }, { ...t, workQuota: o }, s, r), quota: o };
}
function Ac(e, t, n = t.focus) {
  let r = e.plan.map((t, n) => ji(e, e.round, n) ?? null),
    i = hc(e),
    a = kc(e, t, r, i, n, yc);
  return {
    state: oo(e, a.plan),
    meta: Oc(
      {
        ...fc(t),
        focus: n,
        round: e.round,
        free: [],
        auto: i,
        customized: !1,
        relaxed: [],
        workQuota: a.quota,
      },
      a.info,
    ),
    protection: a.info,
  };
}
function jc(e, t, n = t.focus) {
  let r = t.auto.filter((t) => t >= z(e) && !ji(e, e.round, t)),
    i = kc(
      e,
      t,
      e.plan.map((e, t) => (r.includes(t) ? null : e)),
      r,
      n,
      bc(e),
    ),
    a = r
      .filter((t) => i.plan[t] !== e.plan[t])
      .map((t) => ({ index: t, from: e.plan[t], to: i.plan[t] }));
  return {
    state: a.length ? oo(e, i.plan) : e,
    meta: Oc({ ...fc(t), focus: n, workQuota: i.quota }, i.info),
    changes: a,
    protection: i.info,
  };
}
function Mc(e, t, n, r) {
  return jc(e, {
    ...fc(t),
    conflicts: [
      ...t.conflicts.filter((e) => e.key !== n),
      { key: n, choice: r },
    ],
  });
}
function Nc(e, t, n, r) {
  if (n < z(e) || ji(e, e.round, n))
    return {
      state: e,
      meta: t,
      error: `这个半日已经过去或有固定课程／考试，不能作为特别安排。`,
    };
  if (!t.free.includes(n) && t.free.length >= 2)
    return { state: e, meta: t, error: `特别安排最多 2 个半日。` };
  if (P(r) || r === null)
    return {
      state: e,
      meta: t,
      error: `全天游玩要占同一天两个半日，请在“详细安排”中安排。`,
    };
  let i = no(e, n, r);
  if (i.error) return { state: e, meta: t, error: i.error };
  let a = {
      ...fc(t),
      free: [...new Set([...t.free, n])].sort((e, t) => e - t),
      auto: t.auto.filter((e) => e !== n),
      relaxed: t.relaxed.filter((e) => e !== n),
    },
    o = jc(oo(e, i.plan), a);
  return { state: o.state, meta: o.meta, error: null };
}
function Pc(e, t, n) {
  if (!t.free.includes(n) || n < z(e))
    return { state: e, meta: t, error: `这个特别安排已经执行，不能取消。` };
  let r = jc(e, {
    ...fc(t),
    free: t.free.filter((e) => e !== n),
    auto: [...t.auto, n].sort((e, t) => e - t),
  });
  return { state: r.state, meta: r.meta, error: null };
}
function Fc(e) {
  let t = [
    `walk`,
    `rest`,
    `review`,
    ...gc(e).map((e) => e.item),
    ...e.jobs.hired.map((e) => `job:${e}`),
  ];
  return [...new Set(t)].map((e) => ({ item: e, label: ir(e) }));
}
function Ic(e, t, n = !1) {
  let r = z(e),
    i = Math.min(e.plan.length, (Math.floor(r / 2) + 2) * 2),
    a = t.auto
      .filter((t) => t >= r && t < i && !ji(e, e.round, t))
      .sort((e, t) => e - t),
    o = Tc(
      e,
      e.plan,
      t.auto.filter((e) => e >= r),
      xc(e, t).items,
    ),
    s = a
      .filter(
        (t) =>
          e.plan[t] !== `rest` &&
          Ec(e, e.plan[t], t, o) >= 0 &&
          Ec(e, e.plan[t], t, o) <= 3,
      )
      .slice(0, 4),
    c = a.filter((t) => mc(e.plan[t]) && Ec(e, e.plan[t], t, o) < 0);
  if (!s.length && c.length && !n) {
    let n = [...new Set(c.map((t) => e.plan[t]))];
    return {
      state: e,
      meta: t,
      changed: [],
      error: null,
      needConfirm: { slots: c.slice(0, 4), items: n },
    };
  }
  if ((!s.length && n && (s = c.slice(0, 4)), !s.length))
    return {
      state: e,
      meta: t,
      changed: [],
      error: `接下来两天没有可以改成休息的半日（已经在休息，或者是课程、考试、特别安排和你亲手安排的事）。`,
      needConfirm: null,
    };
  let l = [...e.plan];
  for (let e of s) l[e] = `rest`;
  let u = {
      ...fc(t),
      auto: t.auto.filter((e) => !s.includes(e)),
      relaxed: [...new Set([...t.relaxed, ...s])],
    },
    d = jc(oo(e, l), u);
  return {
    state: d.state,
    meta: d.meta,
    changed: s,
    error: null,
    needConfirm: null,
  };
}
function G(e, t, n) {
  let r = n.auto.filter((n) => e.plan[n] !== t.plan[n]);
  return r.length
    ? { ...fc(n), auto: n.auto.filter((e) => !r.includes(e)), customized: !0 }
    : n;
}
function Lc(e, t) {
  let n = [];
  for (let r = z(e); r < e.plan.length; r++) e.plan[r] === t && n.push(r);
  return n;
}
function Rc(e) {
  for (let t = z(e); t < e.plan.length - 1; t++) {
    if (t % 2 != 0) continue;
    let n = P(e.plan[t]);
    if (n && e.plan[t + 1] === e.plan[t]) return { am: t, loc: n };
  }
  return null;
}
function zc(e, t, n, r) {
  let i = (n) => ({ state: e, meta: t, error: n }),
    a = Rc(e);
  if (!a || a.am !== n) return i(`这一天没有尚未出发的全天游玩。`);
  if (a.loc === r) return i(`已经是这个地点。`);
  let o = no(e, n, kn(r));
  if (o.error) return i(o.error);
  let s = oo(e, o.plan);
  return { state: s, meta: G(e, s, t), error: null };
}
var Bc = (e) => Math.max(0, Math.min(100, 100 - e)),
  Vc = (e) => (e === 0 ? 0 : -e),
  Hc = (e) => (e > 0 ? `+${e}` : e < 0 ? `−${-e}` : `±0`),
  Uc = 100 - F.illness,
  Wc = F.illness,
  Gc = 100 - Fn.penaltyFrom;
function Kc(e) {
  let t = Bc(e);
  return t <= 10
    ? { label: `优先休整`, tone: `risk` }
    : t <= 20
      ? { label: `需要休息`, tone: `risk` }
      : t < 40
        ? { label: `有些疲惫`, tone: `low` }
        : t < 70
          ? { label: `状态尚可`, tone: `ok` }
          : { label: `精力充沛`, tone: `ok` };
}
function qc(e) {
  return e >= 90
    ? { label: `优先休整`, tone: `risk` }
    : e >= 80
      ? { label: `需要放松`, tone: `risk` }
      : e > 60
        ? { label: `有点紧绷`, tone: `low` }
        : e >= 30
          ? { label: `平稳`, tone: `ok` }
          : { label: `轻松`, tone: `ok` };
}
function Jc(e, t = ``) {
  return [
    e.fatigue ? `精力 ${Hc(Vc(e.fatigue))}` : ``,
    e.stress ? `压力 ${Hc(e.stress)}` : ``,
    e.money ? `HK$ ${Hc(e.money)}` : ``,
    e.affection ? `${t}好感 ${Hc(e.affection)}` : ``,
    e.reputation ? `声望 ${Hc(e.reputation)}` : ``,
  ]
    .filter(Boolean)
    .join(`、`);
}
var Yc = (e) =>
    e
      .replace(/[：:，,]?\s*疲劳 [+−-]\d+(?:、压力 [+−-]\d+)?/g, ``)
      .replace(/。。/g, `。`),
  Xc = (e) => [...e.history, ...e.logs];
function Zc(e) {
  let t = Xc(e).at(-1);
  return t
    ? {
        energy: Vc(t.fatigue),
        stress: t.stress,
        when: `${t.date}${t.half ? Qn[t.half] : ``}`,
        title: t.title,
      }
    : null;
}
var Qc = (e) =>
  `现金 ${Hc(e.money)} · 精力 ${Hc(Vc(e.fatigue))} · 压力 ${Hc(e.stress)}`;
function $c(e, t, n, r) {
  let i = ua(e, n, t),
    a = H(n, t),
    o = { component: n, name: Ta(n), weight: r },
    s = I(da(t, n));
  switch (i.status) {
    case `unreleased`:
      return {
        ...o,
        state: `unreleased`,
        score: null,
        text: `尚未发布（第 ${Tn(t)[n].releaseWeek} 教学周课堂上发布）`,
      };
    case `active`:
      return {
        ...o,
        state: `working`,
        score: null,
        text: `进行中 · 进度 ${i.progress}/${a} · ${s} 截止 · 待评估`,
      };
    case `complete`: {
      let e = ba(i);
      return {
        ...o,
        state: `complete`,
        score: null,
        text: `已完成、未提交 · 作品质量 ${i.quality}${e ? `（预计 ${Er(e)}）` : ``}，提交后才计分，还不是课程成绩`,
      };
    }
    case `overdue`:
      return {
        ...o,
        state: `overdue`,
        score: null,
        text: `已过截止，补交中 · 进度 ${i.progress}/${a} · ${I(fa(t, n))} 前完成按 ×0.8 计分，否则 0 分`,
      };
    case `missing`:
      return {
        ...o,
        state: `zero`,
        score: 0,
        text: `缺交 · 0 分，按原权重 ${r}% 计入`,
      };
    case `submitted`:
      return i.score === null
        ? {
            ...o,
            state: `unknown`,
            score: null,
            text: `旧档作品，分数无法还原`,
          }
        : {
            ...o,
            state: `scored`,
            score: i.score,
            text: `${i.late ? `迟交` : `按时`}提交 · 得分 ${i.score}`,
          };
  }
}
function el(e, t, n, r) {
  let i = ra(t, n),
    a = e.exams[i.id],
    o = { component: n, name: Ta(n), weight: r };
  if (!a)
    return {
      ...o,
      state: `scheduled`,
      score: null,
      text: `${I(i.day)}${Qn[i.half]}考试 · 待评估`,
    };
  if (a.legacy)
    return {
      ...o,
      state: `unknown`,
      score: null,
      text: `旧档场次，考前状态没有记录，分数无法还原`,
    };
  if (a.status === `taken`)
    return { ...o, state: `scored`, score: a.score, text: `得分 ${a.score}` };
  if (a.status === `missed`)
    return {
      ...o,
      state: `zero`,
      score: 0,
      text: `无故缺考 · 0 分，按原权重 ${r}% 计入`,
    };
  if (a.makeup === `pending`) {
    let e = ki(i.id);
    return {
      ...o,
      state: `makeup`,
      score: null,
      text: `病假缺考，待补考（${I(e.day)}${Qn[e.half]}），不按 0 分处理`,
    };
  }
  return a.makeup === `taken`
    ? {
        ...o,
        state: `scored`,
        score: a.score,
        text: `病假后补考 · 得分 ${a.score}`,
      }
    : {
        ...o,
        state: `zero`,
        score: 0,
        text: `病假后未参加补考 · 0 分，按原权重 ${r}% 计入`,
      };
}
var tl = [
  `overdue`,
  `makeup`,
  `complete`,
  `working`,
  `scheduled`,
  `unreleased`,
];
function nl(e, t) {
  let n = Ea(e, t),
    r = Object.keys(n.weights).map((r) =>
      r === `individual` || r === `group`
        ? $c(e, t, r, n.weights[r])
        : el(e, t, r, n.weights[r]),
    ),
    i = r.filter((e) => e.score !== null).reduce((e, t) => e + t.weight, 0),
    a = Ca(n.counted),
    o = tl.map((e) => r.find((t) => t.state === e)).find(Boolean),
    s = {
      parts: r,
      covered: i,
      counted: a,
      next: n.status === `in_progress` && o ? `${o.name}：${o.text}` : null,
      reasons: n.reasons,
      attendanceRisk: n.status === `in_progress` && n.attendanceRisk,
    };
  return n.status === `official`
    ? {
        ...s,
        status: `official`,
        failed: !n.result.passed,
        headline: `正式成绩 ${n.total === null ? `—` : Ca(n.total)}（${Er(n.result.letter)}，绩点 ${(n.result.points100 / 100).toFixed(2)}）`,
      }
    : n.status === `unknown`
      ? {
          ...s,
          status: `unknown`,
          failed: !1,
          headline: `无法评估：有旧档分数无法还原`,
        }
      : i === 0
        ? { ...s, status: `none`, failed: !1, headline: `尚未评估` }
        : {
            ...s,
            status: `partial`,
            failed: !1,
            headline: `已评定部分 · 覆盖课程权重 ${i}%${i === 100 ? `（正式成绩在学期结束后公布）` : ``}`,
          };
}
var rl = (e) => Dl(e).slice(0, 3);
function il(e) {
  let t = [],
    n = Mi(e),
    r = M[e.round],
    i = N.filter((e) => e.semester === (r.semester ?? 0));
  for (let r of N) {
    for (let i of [`individual`, `group`]) {
      let a = ua(e, i, r.id),
        o =
          i === `individual` ? `「${r.assignment}」` : `「${r.name}」小组项目`;
      a.status === `overdue`
        ? t.push(
            `${o}已过截止：${I(fa(r.id, i))} 前完成按 ×0.8 计分，否则 0 分（进度 ${a.progress}/${H(i, r.id)}）。`,
          )
        : a.status === `active` &&
          da(r.id, i) - n <= 7 &&
          t.push(
            `${o}${I(da(r.id, i))} 截止，进度 ${a.progress}/${H(i, r.id)}。`,
          );
    }
    for (let n of [`midterm`, `final`]) {
      let i = ra(r.id, n),
        a = i ? e.exams[i.id] : null;
      if (i && a?.makeup === `pending`) {
        let e = ki(i.id);
        t.push(
          `${r.name}${Ta(n)}待补考：${I(e.day)}${Qn[e.half]}（改成其他安排会记为缺考）。`,
        );
      }
    }
  }
  for (let n of i)
    Ea(e, n.id).status === `in_progress` &&
      Ea(e, n.id).attendanceRisk &&
      t.push(`${n.name}出勤率目前低于 70%：学期结束时仍不足会强制 F。`);
  let a = e.logs.filter((e) => e.text.includes(`记为无故缺勤`)).length,
    o = e.logs.filter(
      (e) => e.text.includes(`无故缺考`) || e.text.includes(`记为缺考`),
    ).length;
  a && t.push(`本轮有 ${a} 次课堂记为无故缺勤。`),
    o && t.push(`本轮有 ${o} 场考试记为缺考（0 分）。`);
  let s = e.logs.filter((e) => e.title === `获批病假 · 在家休养`).length;
  s && t.push(`本轮因病休养 ${s} 天（医疗费各 HK$${F.medical}）。`);
  let c = Bc(e.fatigue);
  return (
    c <= Uc
      ? t.push(
          `精力 ${c}：若下一天开始时仍在 ${Uc} 或以下（即疲劳 ≥ ${F.illness}），将安排一整天病假。`,
        )
      : c <= 20 && t.push(`精力 ${c}，需要休息。`),
    e.stress >= Wc
      ? t.push(
          `压力 ${e.stress}：若下一天开始时仍在 ${Wc} 或以上，将安排一整天病假。`,
        )
      : e.stress >= 80 && t.push(`压力 ${e.stress}，需要放松。`),
    t.push(...al(e)),
    t
  );
}
function al(e) {
  if (e.phase !== `planning` && e.phase !== `running`) return [];
  let t = Mi(e),
    n = e.phase === `planning` ? 0 : e.cursor;
  return e.plan.some(
    (r, i) =>
      i >= n &&
      (r?.startsWith(`exam:`) || r?.startsWith(`makeup:`)) &&
      M[e.round].startDay + Math.floor(i / 2) <= t + 2,
  )
    ? lc(e)
        .exams.filter(
          (e) =>
            e.day <= t + 2 &&
            (e.fatigue > Fn.penaltyFrom || e.stress > Fn.penaltyFrom),
        )
        .map((e) => {
          let t = N.find((t) => t.id === e.course),
            n = e.kind === `makeup` ? `补考` : Ta(e.kind);
          return `预测：${I(e.day)}${Qn[e.slot % 2 == 0 ? `am` : `pm`]}${t.name}${n}开考前精力约 ${Bc(e.fatigue)}、压力约 ${e.stress}。精力低于 ${Gc} 或压力高于 ${Fn.penaltyFrom} 的部分会影响分数（以开考前的实际状态为准），可以考虑考前多休息。`;
        })
    : [];
}
var ol = (e) =>
    e.round >= 3 && (e.phase === `summary` || e.phase === `finished`),
  sl = (e, t) => e.replaceAll(`{npc}`, t),
  cl = (e) => [...e.history, ...e.logs];
function ll(e) {
  let t = [];
  for (let n of e.events) {
    if (n.choice === null) continue;
    let r = Br(n.id);
    if (!r) continue;
    let i = n.npc ? (pa(e, n.npc)?.name ?? `组员`) : ``,
      a = r.options[n.choice];
    t.push({
      key: ul(n),
      round: n.round,
      day: n.day,
      kind: r.trigger,
      title: sl(r.title, i),
      text: `${sl(a.label, i)}——${sl(a.result, i)}`,
      cg: r.cg,
    });
  }
  for (let n of N) {
    if (!e.projects[n.id].locked) continue;
    let r = cl(e).find((e) => e.title === `小组作业 · ${n.name}`),
      i = r
        ? M.find((e) => r.day >= e.startDay && r.day <= e.endDay).index
        : e.round;
    t.push({
      key: dl(n.id),
      round: i,
      day: r?.day ?? 0,
      kind: `team`,
      title: `和同学开始「${n.name}」小组项目`,
      text: fl(e, n.id),
      cg: null,
    });
  }
  return t.sort((e, t) => e.day - t.day);
}
var ul = (e) => `event:${e.round}:${e.day}:${e.id}`,
  dl = (e) => `team:${e}`;
function fl(e, t) {
  let n = e.projects[t],
    r = Bn.map((r) => {
      let i = n.roles[r.id];
      return `${i === `self` ? `你` : (pa(e, i)?.name ?? `组员`)}负责${mr(t, r.id)}`;
    });
  return `${n.members.map((t) => pa(e, t)?.name).join(`、`)}和你一起开工：${r.join(`，`)}。目前进度 ${n.progress}。`;
}
function pl(e, t) {
  for (let n of ll(e)) {
    if (t.seen.includes(n.key)) continue;
    if (n.kind === `team`)
      return {
        key: n.key,
        title: `合作开始了`,
        lines: [n.text],
        memory: n.title,
        delta: null,
        cg: null,
      };
    let r = e.events.find((e) => ul(e) === n.key),
      i = Br(r.id).options[r.choice],
      a = cl(e).find((e) => e.day === r.day && e.text.includes(`【${n.title}`)),
      o = Jc(i);
    return {
      key: n.key,
      title: n.title,
      lines: [
        n.text,
        `这个选项本身：${o || `没有数值变化`}（叠加在这个半日的活动上）。`,
      ],
      memory: n.title,
      delta: a
        ? { money: a.money, fatigue: a.fatigue, stress: a.stress }
        : null,
      cg: n.cg,
    };
  }
  return null;
}
var ml = (e, t) => (e.seen.includes(t) ? e : { ...e, seen: [...e.seen, t] });
function hl(e) {
  let t = N.filter((t) => t.semester === (M[e.round].semester ?? 1));
  return (
    t
      .filter((t) => e.projects[t.id].released !== null)
      .sort(
        (t, n) => e.projects[t.id].released - e.projects[n.id].released,
      )[0] ?? t[0]
  ).id;
}
var gl = (e, t) =>
    `${I(M[e.round].startDay + Math.floor(t / 2))}${Qn[t % 2 == 0 ? `am` : `pm`]}`,
  _l = [
    `在香港走一走，留下第一份生活回忆`,
    `找到合作伙伴，开始第一份小组任务`,
    `把第一份个人作业按时交出去`,
    `参加期中考试，小组项目继续推进`,
  ];
function vl(e) {
  return e.round < 4
    ? yl(e, e.round)
    : {
        title: `照自己的节奏过好这两周`,
        done: !1,
        status: `current`,
        next: `按本轮安排继续。`,
        note: null,
        detail: [],
      };
}
function yl(e, t) {
  let n = _l[t],
    r = (e, t, r = null) => ({
      title: n,
      done: e === `done`,
      status: e,
      next: t,
      note: r,
      detail: [],
    });
  if (t > e.round) return r(`upcoming`, ``);
  let i = M[t].endDay,
    a = t < e.round || e.phase === `summary` || e.phase === `finished`,
    o = (e, t, n, i, o) =>
      e ? r(`done`, t) : a ? r(`missed`, n, i) : r(`current`, o);
  if (t === 0) {
    let n = ll(e).find((e) => e.kind === `walk`),
      r = !!n && n.day <= i,
      a = t === e.round ? Lc(e, `walk`) : [],
      s = e.firstWalkDone
        ? `散步途中遇到了一件事，选择后会记下回忆。`
        : a.length
          ? `${gl(e, a[0])}外出散步（HK$45，精力 +4、压力 −8）。`
          : `本轮还没有安排散步：在“特别安排”里加一次“外出散步”，就会出门走走；不去也可以，不会凭空记下回忆。`;
    return o(
      r,
      `已留下回忆：「${n?.title}」。`,
      `这一轮没有出门散步，没有留下第一份生活回忆。`,
      n ? `后来在 ${I(n.day)} 留下了回忆：「${n.title}」。` : null,
      s,
    );
  }
  return t === 1 ? xl(e, i, a, r) : t === 2 ? Cl(e, i, a, r) : wl(e, i, a, r);
}
function bl(e) {
  let t = N.filter((t) => e.projects[t.id].locked).map(
    (t) => cl(e).find((e) => e.title === `小组作业 · ${t.name}`)?.day ?? null,
  );
  return {
    any: t.length > 0,
    day: t.some((e) => e === null) ? null : t.length ? Math.min(...t) : null,
  };
}
function xl(e, t, n, r) {
  let i = bl(e),
    a = N.find((t) => e.projects[t.id].locked);
  if (i.any && i.day === null && n)
    return r(
      `unknown`,
      `旧存档没有记录小组作业开始的日期，无法判断是否在这一轮开始。`,
    );
  if (i.any && (i.day === null || i.day <= t))
    return r(
      `done`,
      `已和 ${e.projects[a.id].members.map((t) => pa(e, t)?.name).join(`、`)} 开始「${a.name}」小组项目。`,
    );
  if (n)
    return r(
      `missed`,
      `这一轮没有开始小组作业。`,
      i.any ? `后来在 ${I(i.day)} 开始了小组作业。` : null,
    );
  let o = hl(e),
    s = e.projects[o],
    c = N.find((e) => e.id === o);
  if (s.status === `unreleased`) {
    let t = Tn(o).group.releaseWeek,
      n = M[e.round].semester ?? 1;
    return {
      ...r(
        `current`,
        `小组项目在第 ${t} 教学周的课堂上发布（本轮后半，最早是${c.name}，约 ${I(En(o, t) - 6)}）。发布前不能提前开工。`,
      ),
      detail: [
        `第 ${n} 学期各课的个人作业在第 3 周、小组项目在第 4 周按课表发布。`,
      ],
    };
  }
  if (!ma(s))
    return r(
      `current`,
      `「${c.name}」小组项目已发布：接受默认队伍，或到“小组项目”手动邀请同学。`,
    );
  let l = Lc(e, nr(o));
  return r(
    `current`,
    l.length
      ? `已安排 ${l.length} 个小组作业半日，第一次在 ${gl(e, l[0])}（当前进度 ${s.progress}）。`
      : `队伍已组好，但本轮剩下的半日里还没有小组作业：请在特别安排或“详细安排”中放一个“小组作业”。`,
  );
}
var Sl = (e, t) => {
  let n = e.tasks[t];
  return (
    n.status === `submitted` &&
    !n.late &&
    n.submittedDay !== null &&
    n.submittedDay <= da(t, `individual`)
  );
};
function Cl(e, t, n, r) {
  let i = M[2],
    a = N.filter(
      (e) => e.semester === i.semester && da(e.id, `individual`) <= t,
    );
  if (!a.length) return r(`unknown`, `这一轮没有截止的个人作业。`);
  let o = (t) => {
    let n = e.tasks[t.id];
    return Sl(e, t.id)
      ? `「${t.assignment}」按时提交`
      : n.status === `submitted`
        ? `「${t.assignment}」${I(n.submittedDay)}迟交补交`
        : n.status === `missing`
          ? `「${t.assignment}」缺交`
          : `「${t.assignment}」未按时提交`;
  };
  if (a.every((t) => Sl(e, t.id))) return r(`done`, a.map(o).join(`；`) + `。`);
  if (n) {
    let n = a
        .filter(
          (n) =>
            !Sl(e, n.id) &&
            e.tasks[n.id].status === `submitted` &&
            e.tasks[n.id].submittedDay > t,
        )
        .map(
          (t) =>
            `「${t.assignment}」后来在 ${I(e.tasks[t.id].submittedDay)} 迟交补交（不算按时）`,
        ),
      i = a
        .filter((t) => !Sl(e, t.id))
        .map((n) => {
          let r = e.tasks[n.id];
          return r.status === `submitted` && r.submittedDay <= t
            ? `「${n.assignment}」${I(r.submittedDay)}已补交（迟交）`
            : `「${n.assignment}」本轮结束时仍未提交`;
        });
    return r(
      `missed`,
      [...a.filter((t) => Sl(e, t.id)).map(o), ...i].join(`；`) + `。`,
      n.length ? n.join(`；`) + `。` : null,
    );
  }
  let s = a.find((t) => !Sl(e, t.id)),
    c = e.tasks[s.id];
  return r(
    `current`,
    c.status === `complete`
      ? `「${s.assignment}」已完成，${I(da(s.id, `individual`))} 截止时自动提交（也可以先打磨或手动提交）。`
      : c.status === `overdue`
        ? `「${s.assignment}」已过截止，进入补交期（完成后按 ×0.8 计分，不算按时）。`
        : c.status === `submitted`
          ? `「${s.assignment}」已迟交补交，不算按时。`
          : `「${s.assignment}」进度 ${c.progress}/${H(`individual`, s.id)}，${I(da(s.id, `individual`))} 截止；已安排 ${Lc(e, tr(s.id)).length} 个作业半日。`,
  );
}
function wl(e, t, n, r) {
  let i = M[3],
    a = N.flatMap((e) => {
      let n = ra(e.id, `midterm`);
      return n && n.day >= i.startDay && n.day <= t ? [{ c: e, e: n }] : [];
    });
  if (!a.length) return r(`unknown`, `这一轮没有期中考试。`);
  let o = (t) => e.exams[t],
    s = ({ c: e, e: t }) => {
      let n = o(t.id);
      return n
        ? n.status === `taken`
          ? `${e.name}期中得分 ${n.score}`
          : n.status === `excused`
            ? n.makeup === `taken`
              ? `${e.name}期中病假后补考完成`
              : n.makeup === `missed`
                ? `${e.name}期中病假后补考缺考`
                : `${e.name}期中病假，待补考`
            : `${e.name}期中缺考（0 分）`
        : `${e.name}期中在 ${I(t.day)}${Qn[t.half]}`;
    },
    c = (e) =>
      o(e)?.status === `taken` ||
      (o(e)?.status === `excused` && o(e).makeup === `taken`),
    l = a.map(s).join(`；`) + `。`;
  if (a.every(({ e }) => c(e.id))) return r(`done`, l);
  if (
    a.some(
      ({ e }) => o(e.id)?.status === `missed` || o(e.id)?.makeup === `missed`,
    )
  )
    return r(n ? `missed` : `current`, l);
  if (a.some(({ e }) => o(e.id)?.makeup === `pending`))
    return r(n || a.every(({ e }) => o(e.id)) ? `pending` : `current`, l);
  if (n) return r(`unknown`, l);
  let u = a.find(({ e }) => !o(e.id));
  return r(
    `current`,
    `${I(u.e.day)}${Qn[u.e.half]}是${u.c.name}期中考试；考试按开考前的真实精力和压力计分。${
      a.some(({ e }) => o(e.id))
        ? `已考：${a
            .filter(({ e }) => o(e.id))
            .map(s)
            .join(`；`)}。`
        : ``
    }`,
  );
}
var Tl = {
  review: `这段内容，终于不像刚打开时那么陌生了。`,
  rest: `今天给自己留了一点空白。`,
  tired: `先缓一缓，明天再继续。`,
  team: `有人一起分担，事情也有了眉目。`,
  walk: `走出门，这座城市好像近了一点。`,
  job: `这里的第一份工资到手了，日子有了实在的分量。`,
};
function El(e, t) {
  let n = t.monologues.find((t) => t.round === e.round - 1)?.key,
    r = e.roundStart.fatigue,
    i =
      e.logs.reduce((e, t) => Math.max(0, Math.min(100, e + t.fatigue)), r) ===
      e.fatigue,
    a = new Map(),
    o = e.history;
  for (let t of e.logs) {
    r = Math.max(0, Math.min(100, r + t.fatigue));
    let s = [];
    if (t.title === `预习与复习`) {
      let e = t.text.match(/复习(.+?)，/)?.[1] ?? ``;
      a.set(e, (a.get(e) ?? 0) + 1), a.get(e) === 2 && s.push(`review`);
    }
    t.title === `在家休息` && -t.fatigue >= 5 && s.push(`rest`),
      (t.title.startsWith(`获批病假`) || (i && 100 - r < 30)) &&
        s.push(`tired`),
      t.title.startsWith(`小组作业 ·`) &&
        !o.some((e) => e.title.startsWith(`小组作业 ·`)) &&
        e.logs.find((e) => e.title.startsWith(`小组作业 ·`)) === t &&
        s.push(`team`),
      (t.title === `外出散步` ||
        (t.title.startsWith(`全天游玩 ·`) && t.half === `pm`)) &&
        s.push(`walk`),
      t.title.startsWith(`兼职 ·`) &&
        ![...o, ...e.logs.slice(0, e.logs.indexOf(t))].some((e) =>
          e.title.startsWith(`兼职 ·`),
        ) &&
        s.push(`job`);
    let c = s.find((e) => e !== n);
    if (c) return { key: c, text: Tl[c], day: t.day };
  }
  return null;
}
function Dl(e) {
  let t = e.logs,
    n = (e) => t.filter(e).length,
    r = n((e) => N.some((t) => e.title === t.name)),
    i = n((e) => e.title.startsWith(`考试 ·`) || e.title.startsWith(`补考 ·`)),
    a = n(
      (e) =>
        e.title.startsWith(`个人作业 ·`) || e.title.startsWith(`小组作业 ·`),
    ),
    o = n((e) => e.title === `预习与复习`),
    s = n((e) => e.title === `外出散步`),
    c = n((e) => e.title.startsWith(`全天游玩 ·`) && e.half === `pm`),
    l = [
      r ? `上课 ${r} 次` : ``,
      i ? `考试 ${i} 场` : ``,
      a ? `作业 ${a} 个半日` : ``,
      o ? `复习 ${o} 次` : ``,
      s ? `散步 ${s} 次` : ``,
      c ? `出游 ${c} 天` : ``,
    ].filter(Boolean),
    u = [];
  l.length && u.push({ label: `做成了什么`, text: l.join(`、`) + `。` });
  let d = ll(e).filter((t) => t.round === e.round),
    f = d.find((e) => e.kind === `team`);
  f && u.push({ label: `与谁开始合作`, text: f.text });
  let p = d.filter((e) => e.kind !== `team`);
  return (
    p.length &&
      u.push({
        label: `留下的回忆`,
        text: p.map((e) => `「${e.title}」`).join(`、`),
      }),
    u.slice(0, 3)
  );
}
function Ol(e) {
  if (e.phase !== `summary` && e.phase !== `finished`) return null;
  let t = e.money - e.roundStart.money,
    n = Math.abs(t).toLocaleString(`en-US`);
  return {
    net: t,
    text:
      t > 0
        ? `本轮现金净增加 HK$${n}`
        : t < 0
          ? `本轮现金净减少 HK$${n}`
          : `本轮收支持平`,
  };
}
var kl = (e) => structuredClone(e),
  Al = { economy: `balanced`, housing: `standard` };
function jl(e, t, n) {
  let r = Pi({ ...e, name: t.trim().slice(0, 20) || `新同学` }, n).state,
    i = Ni(r, yi());
  if (i.error) return { state: e, error: i.error };
  let a = Fi(i.state, Al.economy, Al.housing);
  return a.error
    ? { state: e, error: a.error }
    : ((r = a.state), { state: r, error: null });
}
function Ml(e) {
  let t = M[e.round].semester ?? 1;
  return N.filter((e) => e.semester === t)
    .map((e) => e.id)
    .filter((t) => {
      let n = e.projects[t];
      return (
        n.status !== `unreleased` &&
        !ma(n) &&
        !La(e, t) &&
        n.status !== `submitted` &&
        n.status !== `missing`
      );
    });
}
function Nl(e, t, n) {
  let r = (n) => ({ state: e, meta: t, error: n, names: [] }),
    i = La(e, n);
  if (i) return r(i);
  let a = e.projects[n];
  if (a.members.length >= 2) return r(`队伍已经满员。`);
  let o = 2 - a.members.length,
    s = a.shown.filter((e) => !a.members.includes(e) && !(e in a.invites)),
    c = [...s, ...Fa(e, n).filter((e) => !s.includes(e))].slice(0, o);
  if (c.length < o) return r(`没有足够的候选同学，请到“小组项目”页公开招募。`);
  let l = kl(e),
    u = l.projects[n];
  for (let e of c) (u.invites[e] = !0), u.members.push(e);
  (u.roles = za(l, n)), (l.known = [...new Set([...l.known, ...c])]);
  let d = jc(l, t);
  return {
    state: d.state,
    meta: {
      ...d.meta,
      defaultTeams: [...new Set([...d.meta.defaultTeams, n])],
    },
    error: null,
    names: c.map((e) => pa(l, e).name),
  };
}
function Pl(e, t) {
  let n = e,
    r = t,
    i = [],
    a = [];
  for (let t of Ml(e)) {
    let e = Nl(n, r, t);
    e.error
      ? a.push(`${N.find((e) => e.id === t).name}：${e.error}`)
      : ((n = e.state),
        (r = e.meta),
        i.push(`${N.find((e) => e.id === t).name}（${e.names.join(`、`)}）`));
  }
  return { state: n, meta: r, done: i, errors: a };
}
function Fl(e, t, n) {
  let r =
    M[e.round].startDay + Math.floor(Math.min(e.cursor, e.plan.length - 1) / 2);
  return { ...t, digest: [...t.digest, { round: e.round, day: r, text: n }] };
}
var Il = (e, t) => {
  let n = e.projects[t].released;
  return n !== null && n >= M[e.round].startDay && n <= M[e.round].endDay;
};
function Ll(e, t) {
  let n = e.notice,
    r = N.find((e) => e.id === n.course),
    i = t,
    a = e;
  if (n.kind === `makeup`) {
    let n = t.free.map((t) => e.plan[t]);
    if (
      ((i = Fl(e, i, `${r.name}：病假缺考，补考已排入本轮日程。`)),
      (a = co(a)),
      t.free.find((e, t) => a.plan[e] !== n[t]) !== void 0)
    )
      return {
        state: a,
        meta: i,
        stop: {
          kind: `adjust`,
          message: `补考占用了你的一个特别安排，请确认本轮剩余安排。`,
        },
      };
  } else {
    let t = da(r.id, n.kind);
    (i = Fl(
      e,
      i,
      n.kind === `individual`
        ? `${r.teacher}布置了「${r.assignment}」（${I(t)}截止）。`
        : `${r.name}发布了小组项目（${I(t)}截止）。`,
    )),
      (a = co(a));
  }
  let o = jc(a, i);
  if (
    ((a = o.state),
    (i = o.meta),
    o.changes.length &&
      (i = Fl(
        a,
        i,
        `已为新任务补排 ${o.changes.filter((e) => e.to?.startsWith(`task:`) || e.to?.startsWith(`project:`)).length} 个未来半日。`,
      )),
    n.kind === `group` &&
      !ma(a.projects[n.course]) &&
      !i.teamPrompted.includes(n.course))
  ) {
    let e = i.teamPrompted.some((e) => Il(a, e));
    if (((i = { ...i, teamPrompted: [...i.teamPrompted, n.course] }), !e))
      return {
        state: a,
        meta: i,
        stop: {
          kind: `team`,
          message: `「${r.name}」小组项目发布了：可以直接接受默认队伍，或到“小组项目”手动组队。本轮之后发布的项目会一并列在这里，不再逐个打断。`,
          course: n.course,
        },
      };
  }
  return { state: a, meta: i, stop: null };
}
function Rl(e) {
  if (e.round !== 1) return null;
  let t = hl(e),
    n = e.projects[t];
  return !ma(n) ||
    n.locked ||
    n.status === `submitted` ||
    n.status === `missing` ||
    Lc(e, nr(t)).length
    ? null
    : {
        kind: `adjust`,
        message: `队伍已组好，但本轮剩余的半日里没有小组作业：请在特别安排或“详细安排”中放一个“小组作业”。`,
        course: t,
      };
}
var K = (e) => ({
    kind: `conflict`,
    message: `休整和${e.conflict.items.length > 1 ? `几份` : `一份`}临近截止的作业撞在一起了，请先选一个方向。`,
  }),
  q = (e) => `${e.round}:${e.cursor}`;
function J(e, t, n = {}) {
  let r = Y(e, t, n);
  return r.state.event ? { ...r, meta: Ds(r.state, r.meta) } : r;
}
function Y(e, t, n) {
  let r = e,
    i = t,
    a = 0,
    o = Math.floor(r.cursor / 2);
  for (let e = 0; e < (n.limit ?? 400); e++) {
    if (r.phase === `planning`)
      return {
        state: r,
        meta: i,
        stop: { kind: `planning`, message: `本轮还没有开始。` },
      };
    if ((r.phase === `summary` || r.phase === `finished`) && r.event)
      return {
        state: r,
        meta: i,
        stop: { kind: `event`, message: `遇到了一件事，需要你选择。` },
      };
    if (r.phase === `summary` || r.phase === `finished`) {
      let e = pl(r, i);
      return e
        ? { state: r, meta: i, stop: { kind: `result`, message: e.title } }
        : xs(i)
          ? {
              state: r,
              meta: i,
              stop: { kind: `trip`, message: `旅行回来了。` },
            }
          : {
              state: r,
              meta: i,
              stop: ol(r)
                ? { kind: `sample_end`, message: `四轮样例结束。` }
                : { kind: `round_end`, message: `这一轮结束了。` },
            };
    }
    if (r.notice) {
      let e = Ll(r, i);
      if (((r = e.state), (i = e.meta), e.stop))
        return { state: r, meta: i, stop: e.stop };
      let t = Rl(r);
      if (t) return { state: r, meta: i, stop: t };
      continue;
    }
    let e = pl(r, i);
    if (e)
      return { state: r, meta: i, stop: { kind: `result`, message: e.title } };
    if (r.event)
      return {
        state: r,
        meta: i,
        stop: { kind: `event`, message: `遇到了一件事，需要你选择。` },
      };
    if (r.blockedReason)
      return {
        state: r,
        meta: i,
        stop: { kind: `blocked`, message: r.blockedReason },
      };
    if (i.conflict) return { state: r, meta: i, stop: K(i) };
    if (xs(i))
      return {
        state: r,
        meta: i,
        stop: { kind: `trip`, message: `旅行回来了。` },
      };
    if (n.untilDayEnd && Math.floor(r.cursor / 2) > o)
      return {
        state: r,
        meta: i,
        stop: { kind: `day`, message: `过完了一天。` },
      };
    if (r.cursor % 2 == 0 && i.dayCheck !== q(r)) {
      let e = jc(r, i);
      (r = e.state), (i = { ...e.meta, dayCheck: q(r) });
      continue;
    }
    if (n.steps !== void 0 && a >= n.steps)
      return { state: r, meta: i, stop: { kind: `tick`, message: `继续中。` } };
    let t = r.cursor,
      s = r;
    if (
      ((r = Do(r)),
      a++,
      (i = bs(s, r, i).meta),
      r.cursor === t &&
        !r.event &&
        !r.notice &&
        !r.blockedReason &&
        r.phase === `running`)
    )
      return {
        state: r,
        meta: i,
        stop: { kind: `blocked`, message: `这个半日无法执行，请检查安排。` },
      };
  }
  return {
    state: r,
    meta: i,
    stop: { kind: `limit`, message: `已推进很多步，暂停一下。` },
  };
}
function X(e, t, n) {
  if (e.phase !== `planning` || !Fs(e.round))
    return {
      state: e,
      meta: t,
      changes: 0,
      error: `倾向在每个阶段开始时选择，本阶段沿用已选的倾向。`,
    };
  let r = jc(e, t, n);
  return {
    state: r.state,
    meta: r.meta,
    changes: r.changes.length,
    error: null,
  };
}
function zl(e, t) {
  let n = e,
    r = t;
  return (
    r.round !== n.round && ({ state: n, meta: r } = Ac(n, r)),
    Fs(n.round) && r.focus === `balanced`
      ? {
          state: e,
          meta: t,
          error: `新阶段开始：请先从“学业成长／香港生活／工作体验”中选一个倾向。`,
        }
      : r.conflict
        ? {
            state: e,
            meta: t,
            error: `休整和临近截止的作业有冲突，请先选“先休整”或“继续赶进度”。`,
          }
        : n.plan.some((e) => e === null)
          ? { state: e, meta: t, error: `还有未安排的半日。` }
          : ((r = { ...r, focusPhase: Ps(n.round) }),
            { state: so(n), meta: r, error: null })
  );
}
function Bl(e, t) {
  if (ol(e)) return { state: e, meta: t, error: `四轮样例已经结束。` };
  if (e.phase !== `summary`)
    return { state: e, meta: t, error: `这一轮还没有结束。` };
  if (e.event)
    return { state: e, meta: t, error: `先完成这件事的选择，再进入下一轮。` };
  if (pl(e, t))
    return { state: e, meta: t, error: `先看完这件事的结果，再进入下一轮。` };
  if (xs(t))
    return { state: e, meta: t, error: `先把这次旅行收进手账，再进入下一轮。` };
  let n = El(e, t),
    r = {
      ...t,
      monologues:
        n && !t.monologues.some((t) => t.round === e.round)
          ? [...t.monologues, { round: e.round, key: n.key }]
          : t.monologues,
    },
    i = Ac(Oo(e), r);
  return { state: i.state, meta: i.meta, error: null };
}
function Vl(e, t) {
  if (t.focus !== `work` || e.jobs.hired.length) return null;
  if (xn(e.jobs, e.round))
    return {
      kind: `info`,
      text: `本轮的求职机会已经用过（${e.jobs.lastAttempt.success ? `已录用` : `这次没有被录用`}），下一轮规划时可以再申请。没有兼职的半日照常安排作业与休息。`,
    };
  if (!Ji(e))
    return {
      kind: `info`,
      text: `求职只能在每轮开始前的规划阶段进行；下一轮开始前可以申请。`,
    };
  if (t.jobCardDismissed === e.round) return null;
  let n = mn.filter((t) => vn(t, e.abilities)).sort((e, t) => t.pay - e.pay);
  if (!n.length) {
    let t = [...mn].sort(
      (t, n) => _n(t, e.abilities).length - _n(n, e.abilities).length,
    )[0];
    return {
      kind: `none`,
      text: `目前没有达到条件的岗位（最接近的「${t.name}」还差：${_n(
        t,
        e.abilities,
      )
        .map((e) => `${Jn(e.ability)} ${(e.required - e.current).toFixed(1)}`)
        .join(`、`)}）。这一轮照常安排作业与休息。`,
    };
  }
  let r = n[0];
  return { kind: `offer`, job: r, chance: yn(r, e.abilities) };
}
var Hl = (e, t) => ({ ...t, jobCardDismissed: e.round });
function Ul(e, t) {
  return e.notice
    ? `notice`
    : (e.phase === `summary` || e.phase === `finished`) && e.event
      ? `event`
      : pl(e, t)
        ? `result`
        : e.event
          ? `event`
          : xs(t)
            ? `trip`
            : null;
}
function Wl(e, t, n = {}) {
  let r = Ul(e, t);
  if (r === `notice`)
    return {
      kind: `notice`,
      label: `看看新通知`,
      hint: `老师发布了新任务，确认后再继续。`,
    };
  if (r === `event`)
    return {
      kind: `event`,
      label: `看看发生了什么`,
      hint: `遇到了一件事，需要你选择。`,
    };
  if (r === `result`)
    return { kind: `result`, label: `查看这次结果`, hint: pl(e, t).title };
  if (r === `trip`)
    return {
      kind: `trip`,
      label: `看看这次旅行`,
      hint: `旅行回来了，把今天收进手账。`,
    };
  if (e.phase === `summary` || e.phase === `finished`)
    return ol(e)
      ? {
          kind: `sample_end`,
          label: `回看这段生活`,
          hint: `四轮样例已经结束，不会进入第 5 轮。`,
        }
      : {
          kind: `round_end`,
          label: `看看这两周`,
          hint: `第 ${e.round + 1} 轮结束了，回顾后再开始下一轮。`,
        };
  if (t.conflict)
    return {
      kind: `conflict`,
      label: `处理眼前的事`,
      hint: `休整和临近截止的作业撞在一起了，先选一个方向。`,
    };
  if (e.blockedReason)
    return { kind: `handle`, label: `处理眼前的事`, hint: e.blockedReason };
  let i = n.stop;
  return i &&
    (i.kind === `team` || i.kind === `adjust`) &&
    (i.kind !== `team` || Ml(e).length > 0)
    ? { kind: `handle`, label: `处理眼前的事`, hint: i.message }
    : e.phase === `planning`
      ? (Fs(e.round) && t.focus === `balanced`) ||
        (Fs(e.round) && t.focusPhase !== Ps(e.round) && !n.focusSeen)
        ? {
            kind: `focus`,
            label: `选择这段时间的重心`,
            hint: `${Is(e.round)}开始：学业成长、香港生活或工作体验，选一个倾向。`,
          }
        : {
            kind: `start`,
            label: `开始这两周`,
            hint: `倾向：${Ns(t.focus)}。到下一件需要你决定的事时停下。`,
          }
      : {
          kind: `continue`,
          label: `继续生活`,
          hint: `到下一件需要你决定的事时停下。`,
        };
}
function Gl(e, t) {
  let n = vl(e),
    r = e.plan.length,
    i = e.phase === `summary` || e.phase === `finished`,
    a =
      e.phase === `planning`
        ? { text: `本轮尚未开始`, done: 0 }
        : i
          ? { text: `本轮已结束`, done: r }
          : { text: `本轮已过 ${e.cursor} / ${r} 个半日`, done: e.cursor },
    o = i ? [] : Ml(e),
    s = t.conflict
      ? `休整和临近截止的作业撞在一起了，需要你选一个方向。`
      : o.length
        ? `${o.map((e) => N.find((t) => t.id === e).name).join(`、`)}的小组项目还没有组队。`
        : (il(e)[0] ?? null),
    c = El(e, t);
  return {
    phase: `${Is(e.round) || `样例`} · ${Ns(t.focus)}`,
    round: `第 ${e.round + 1} / 4 轮`,
    progress: { ...a, total: r },
    goal: {
      title: n.title,
      next: n.next,
      done: n.done,
      status: n.status,
      note: n.note,
    },
    attention: s,
    monologue: c?.text ?? null,
  };
}
var Kl = {
  done: `已达成`,
  current: `进行中`,
  missed: `本轮未达成`,
  pending: `待补考`,
  upcoming: `之后的轮次`,
  unknown: `记录不足，无法判断`,
};
function ql(e) {
  return Array.from({ length: 4 }, (t, n) => {
    let r = yl(e, n);
    return {
      round: n,
      title: r.title,
      status: r.status,
      next: r.status === `upcoming` ? null : r.next,
      note: r.note,
      range: `${I(M[n].startDay)}—${I(M[n].endDay)}`,
    };
  });
}
function Jl(e, t) {
  let n = [];
  if (
    (e.notice && n.push(`有一则新的课程通知待确认。`),
    e.event
      ? n.push(`有一件事等你选择。`)
      : pl(e, t)
        ? n.push(`有一件事的结果还没看。`)
        : xs(t) && n.push(`有一次旅行还没收进手账。`),
    t.conflict && n.push(`休整和临近截止的作业撞在一起了，需要你选一个方向。`),
    e.phase !== `summary` && e.phase !== `finished`)
  ) {
    let t = Ml(e);
    t.length && n.push(`${t.length} 个小组项目还没有组队。`);
  }
  return (
    e.blockedReason && n.push(e.blockedReason),
    {
      todo: n,
      notes: [...t.digest]
        .reverse()
        .map((e) => ({
          round: e.round,
          when: `第 ${e.round + 1} 轮 · ${I(e.day)}`,
          text: e.text,
        })),
    }
  );
}
var Yl = (e, t, n, r, i = ``) => ({
    src: `assets/lite-ui/${e}`,
    width: t,
    height: n,
    source: r,
    status: `trial`,
    alt: i,
  }),
  Xl = {
    room_standard: Yl(
      `room_standard.webp`,
      1672,
      941,
      `SC01`,
      `夕阳照进香港出租屋：左侧书桌，中间大窗，右侧床铺`,
    ),
    char_female_young: Yl(
      `char_female_young.webp`,
      436,
      1442,
      `SC02`,
      `绿色开衫的女主角`,
    ),
    char_male_young: Yl(
      `char_male_young.webp`,
      447,
      1505,
      `SC03`,
      `绿色开衫的男主角`,
    ),
    hud_date: Yl(`hud_date.webp`, 520, 184, `HUD01`),
    hud_money: Yl(`hud_money.webp`, 520, 117, `HUD02`),
    hud_energy: Yl(`hud_energy.webp`, 520, 119, `HUD03`),
    hud_stress: Yl(`hud_stress.webp`, 520, 102, `HUD04`),
    icon_messages: Yl(`icon_messages.webp`, 128, 129, `ICON01`),
    icon_review: Yl(`icon_review.webp`, 128, 128, `ICON02`),
    icon_settings: Yl(`icon_settings.webp`, 128, 121, `ICON03`),
    window: Yl(`window.webp`, 1100, 786, `WIDGET01`),
    button_primary: Yl(`button_primary.webp`, 600, 148, `WIDGET02`),
    button_secondary: Yl(`button_secondary.webp`, 420, 121, `WIDGET05`),
  };
function Zl(e, t = `/`) {
  let n = Xl[e].src;
  return (
    globalThis.__HK_INLINE_ASSETS__?.[n] ??
    `${t.endsWith(`/`) ? t : `${t}/`}${n}`
  );
}

function play(seed, opts) {
  let s = B(seed);
  let r = jl(s, '测试同学', Kr(opts.gender || 'female'));
  if (r.error) throw new Error(r.error);
  s = r.state;
  let lite = Ls();
  let a = Ac(s, lite); s = a.state; lite = a.meta;
  const snaps = [], trace = [];
  const snap = (tag) => snaps.push({ tag, state: JSON.parse(JSON.stringify(s)), lite: JSON.parse(JSON.stringify(lite)) });
  for (let guard = 0; guard < 2000; guard++) {
    if (s.phase === 'planning') {
      if (Fs(s.round)) { const x = X(s, lite, opts.focus[Ps(s.round)]); if (!x.error) { s = x.state; lite = x.meta; } }
      if (lite.focus === 'work') {
        const card = Vl(s, lite);
        if (card && card.kind === 'offer') {
          const y = Yi(s, card.job.id, opts.roll);
          if (!y.error) { s = y.state; if (y.attempt.success && lite.round === s.round && s.phase === 'planning') { const z = jc(s, lite); s = z.state; lite = z.meta; } }
        }
      }
      if (opts.extras) {
        if (s.round === 0) { let b = Ao(s, 'bun'); if (!b.error) s = b.state; b = Ao(s, 'plant'); if (!b.error) s = b.state; s = Mo(s, 'plant'); }
        if (s.round === 1) { const slot = hc(s).find((x) => x % 2 === 1 && !lite.free.includes(x) && x > 6); if (slot !== undefined) { const n = Nc(s, lite, slot, 'review'); if (!n.error) { s = n.state; lite = n.meta; } } }
        if (s.round === 2) { let i = Ic(s, lite); if (i.needConfirm) i = Ic(s, lite, true); if (!i.error && !i.needConfirm) { s = i.state; lite = i.meta; } s = jo(s, 'bun'); }
      }
      if (Ml(s).length) { const p = Pl(s, lite); s = p.state; lite = p.meta; }
      if (opts.grind) { const plan = s.plan.map((x, i) => (i >= z(s) && x === 'rest' ? (opts.grind === 'job' && s.jobs.hired.length && i % 3 === 0 ? 'job:' + s.jobs.hired[0] : 'review') : x)); const g = io(s, 'review', 0); const s2 = oo(s, plan); lite = G(s, s2, lite); s = s2; }
      if (lite.conflict) { const m = Mc(s, lite, lite.conflict.key, opts.conflict); s = m.state; lite = m.meta; }
      const zz = zl(s, lite);
      if (zz.error) {
        if (lite.conflict) { const m = Mc(s, lite, lite.conflict.key, opts.conflict); s = m.state; lite = m.meta; continue; }
        throw new Error("start failed: " + zz.error);
      }
      s = zz.state; lite = zz.meta;
    }
    const st = J(s, lite, opts.dayByDay ? { untilDayEnd: true } : {}); s = st.state; lite = st.meta; trace.push(st.stop.kind + '@' + s.round + ':' + s.cursor);
    switch (st.stop.kind) {
      case 'event': {
        const ev = Br(s.event.id); let c = opts.choice % ev.options.length;
        if (ev.options[c].money < 0 && s.money < -ev.options[c].money) c = (c + 1) % ev.options.length;
        lite = Ds(s, lite); const y = yo(s, c); if (y.error) throw new Error(y.error); s = y.state; break;
      }
      case 'result': lite = ml(lite, pl(s, lite).key); break;
      case 'trip': lite = Ss(lite, xs(lite).key); break;
      case 'team': { const p = Pl(s, lite); s = p.state; lite = p.meta; break; }
      case 'conflict': { const m = Mc(s, lite, lite.conflict.key, opts.conflict); s = m.state; lite = m.meta; break; }
      case 'blocked': {
        const plan = [...s.plan]; if (P(plan[s.cursor]) && s.cursor % 2 === 0) plan[s.cursor + 1] = 'rest'; plan[s.cursor] = 'rest';
        const o = s; s = oo(s, plan); lite = G(o, s, lite); break;
      }
      case 'round_end': { snap('round' + s.round); const n = Bl(s, lite); if (n.error) throw new Error(n.error); s = n.state; lite = n.meta; break; }
      case 'sample_end': snap('end'); return { snaps, trace };
      case 'day': if (opts.sickDay !== undefined && Ei(s.round, s.cursor).day === opts.sickDay && s.phase === 'running') { s = { ...structuredClone(s), fatigue: 95 }; } break;
      default: break;
    }
  }
  throw new Error('guard');
}
const cases = JSON.parse(require('fs').readFileSync(process.argv[2], 'utf8'));
const out = cases.map((c) => ({ name: c.name, ...play(c.seed, c) }));
require('fs').writeFileSync(process.argv[3], JSON.stringify(out));
console.log('JS done', out.length, out.map((o) => o.name + ':' + o.trace.length).join(' '));
