//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSISender.h"

@interface KSTerminatedMessageSender : NSObject <KSISender>

- (id)initWithTerminator:(NSString *)terminator;

@end
