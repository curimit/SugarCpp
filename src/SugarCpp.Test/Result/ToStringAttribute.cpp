enum Day {
    Sunday = 0,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
};

const char* ToString(const Day &a) {
    switch (a) {
    case Sunday:
        return "Sunday";

    case Monday:
        return "Monday";

    case Tuesday:
        return "Tuesday";

    case Wednesday:
        return "Wednesday";

    case Thursday:
        return "Thursday";

    case Friday:
        return "Friday";

    case Saturday:
        return "Saturday";
    }
}